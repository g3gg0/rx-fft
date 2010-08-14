using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using LibRXFFT.Libraries.FFTW;
using LibRXFFT.Components.DirectX;
using LibRXFFT.Libraries.Misc;

namespace LibRXFFT.Components.DirectX
{
    public partial class DemodFFTView : Form
    {
        private Mutex FFTLock = new Mutex();
        private FFTTransformer FFTTransformerInput = new FFTTransformer(256);
        private FFTTransformer FFTTransformerTranslated = new FFTTransformer(256);
        private FFTTransformer FFTTransformerFiltered = new FFTTransformer(256);
        private FFTTransformer FFTTransformerDecimated = new FFTTransformer(256);
        private double[] FFTResult = new double[256];

        private bool WindowActivated = false;

        public enum eDataType
        {
            Input,
            Translated,
            Filtered,
            Decimated
        }

        public DemodFFTView()
        {
            InitializeComponent();

            FFTInput.LimiterDisplayEnabled = false;
            FFTTranslated.LimiterDisplayEnabled = false;
            FFTFiltered.LimiterDisplayEnabled = false;

            FFTInput.MainText = "[Input]";
            FFTTranslated.MainText = "[Translated]";
            FFTFiltered.MainText = "[Filtered]"; 

            FFTInput.UserEventCallback = UserEventCallbackFunc;
            FFTTranslated.UserEventCallback = UserEventCallbackFunc;
            FFTFiltered.UserEventCallback = UserEventCallbackFunc;

            AddUserEventCallback(eUserEvent.MouseEnter);
            AddUserEventCallback(eUserEvent.MouseClickRight);
        }

        protected int FFTSize
        {
            get
            {
                return FFTResult.Length;
            }
            set
            {
                lock (FFTLock)
                {
                    FFTTransformerInput = new FFTTransformer(value);
                    FFTTransformerTranslated = new FFTTransformer(value);
                    FFTTransformerFiltered = new FFTTransformer(value);
                    FFTTransformerDecimated = new FFTTransformer(value);
                    FFTResult = new double[value];
                }
            }
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            WindowActivated = true;
            FocusHovered();
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            WindowActivated = false;
        }

        public void UserEventCallbackFunc(eUserEvent evt, double param)
        {
            switch (evt)
            {
                /* when mouse is moved into a plot and we are in foreground, update focus to hovered plot */
                case eUserEvent.MouseEnter:
                    FocusHovered();
                    break;

                /* bring up popup menu. has to be improved */
                case eUserEvent.MouseClickRight:
                    {
                        long freq = GetHovered().CursorFrequency;

                        ContextMenu contextMenu = new ContextMenu();
                        MenuItem menuItem1 = new MenuItem("Frequency: " + FrequencyFormatter.FreqToStringAccurate(freq));
                        MenuItem menuItem2 = new MenuItem("-");
                        MenuItem menuItem3 = new MenuItem("FFT  512");
                        MenuItem menuItem4 = new MenuItem("FFT 1024");
                        MenuItem menuItem5 = new MenuItem("FFT 2048");
                        MenuItem menuItem6 = new MenuItem("FFT 4096");
                        menuItem1.Enabled = false;

                        contextMenu.MenuItems.AddRange(new MenuItem[] { menuItem1, menuItem2, menuItem3, menuItem4, menuItem5, menuItem6 });

                        switch (FFTSize)
                        {
                            case 512:
                                menuItem3.Checked = true;
                                break;
                            case 1024:
                                menuItem4.Checked = true;
                                break;
                            case 2048:
                                menuItem5.Checked = true;
                                break;
                            case 4096:
                                menuItem6.Checked = true;
                                break;
                        }
                        menuItem3.Click += new EventHandler(delegate(object sender, EventArgs e)
                        {
                            FFTSize = 512;
                        });
                        menuItem4.Click += new EventHandler(delegate(object sender, EventArgs e)
                        {
                            FFTSize = 1024;
                        });
                        menuItem5.Click += new EventHandler(delegate(object sender, EventArgs e)
                        {
                            FFTSize = 2048;
                        });
                        menuItem6.Click += new EventHandler(delegate(object sender, EventArgs e)
                        {
                            FFTSize = 4096;
                        });

                        System.Drawing.Point popupPos = this.PointToClient(MousePosition);
                        popupPos.X -= 20;
                        popupPos.Y -= 20;
                        contextMenu.Show(this, popupPos);
                    }
                    break;
            }
        }


        public void AddUserEventCallback(eUserEvent evt)
        {
            FFTInput.EventActions[evt] = eUserAction.UserCallback;
            FFTTranslated.EventActions[evt] = eUserAction.UserCallback;
            FFTFiltered.EventActions[evt] = eUserAction.UserCallback;
        }

        private DirectXFFTDisplay GetHovered()
        {
            if (FFTInput.MouseHovering)
            {
                return FFTInput;
            }
            if (FFTTranslated.MouseHovering)
            {
                return FFTTranslated;
            }
            if (FFTFiltered.MouseHovering)
            {
                return FFTFiltered;
            }

            return FFTFiltered;
        }

        public void FocusHovered()
        {
            if (WindowActivated)
            {
                if (FFTInput.MouseHovering)
                {
                    FFTInput.Focus();
                }
                if (FFTTranslated.MouseHovering)
                {
                    FFTTranslated.Focus();
                }
                if (FFTFiltered.MouseHovering)
                {
                    FFTFiltered.Focus();
                }
            }
        }

        public double SamplingRate
        {
            set
            {
                FFTInput.SamplingRate = value;
                FFTTranslated.SamplingRate = value;
                FFTFiltered.SamplingRate = value;               
            }
        }
        public void ProcessData(double[] iSamples, double[] qSamples, eDataType type)
        {
            DirectXFFTDisplay display;
            FFTTransformer fft;

            switch (type)
            {
                case eDataType.Input:
                    display = FFTInput;
                    fft = FFTTransformerInput;
                    break;
                case eDataType.Translated:
                    display = FFTTranslated;
                    fft = FFTTransformerTranslated;
                    break;
                case eDataType.Filtered:
                    display = FFTFiltered;
                    fft = FFTTransformerFiltered;
                    break;
                default:
                    return;
            }

            if (display.EnoughData)
                return;

            lock (FFTLock)
            {
                int samplePairs = iSamples.Length;

                for (int samplePair = 0; samplePair < samplePairs; samplePair++)
                {
                    double I = iSamples[samplePair];
                    double Q = qSamples[samplePair];

                    fft.AddSample(I, Q);

                    if (fft.ResultAvailable)
                    {
                        fft.GetResultSquared(FFTResult);
                        display.ProcessFFTData(FFTResult, 0, 0);
                    }
                }
            }
        }
    }
}
