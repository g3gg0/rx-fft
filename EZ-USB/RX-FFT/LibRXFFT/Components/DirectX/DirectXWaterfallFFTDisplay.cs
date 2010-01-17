﻿using System.Threading;
using System.Windows.Forms;
using LibRXFFT.Libraries;
using LibRXFFT.Libraries.FFTW;
using System;
using LibRXFFT.Libraries.GSM.Misc;
using System.Drawing;
using System.Collections.Generic;

namespace LibRXFFT.Components.DirectX
{
    public partial class DirectXWaterfallFFTDisplay : UserControl
    {
        public UserEventCallbackDelegate UserEventCallback;

        private Color MarkerColor = Color.LightGreen;

        private Mutex FFTLock = new Mutex();
        private FFTTransformer FFT = new FFTTransformer(256);
        private int _FFTSize = 256;
        private double[] FFTResult = new double[256];
        private double[] FFTResultPartial = new double[256];

        private bool LevelBarActive = false;
        private LabelledLine LevelBarUpper = new LabelledLine("Waterfall upper limit", 0, Color.BlueViolet);
        private LabelledLine LevelBarLower = new LabelledLine("Waterfall lower limit", 0, Color.BlueViolet);


        private double IMax = -100;
        private double IMin = 100;


        public DirectXWaterfallFFTDisplay()
        {
            InitializeComponent();

            /* we work with already squared FFT values for performance reasons */
            FFTDisplay.SquaredFFTData = true;
            WaterfallDisplay.SquaredFFTData = true;

            /* handle some actions ourselves */
            FFTDisplay.UserEventCallback = UserEventCallbackFunc;
            WaterfallDisplay.UserEventCallback = UserEventCallbackFunc;

            /* when dragging in Y-direction in FFTDisplay, update offset */
            FFTDisplay.EventActions[eUserEvent.MouseDragY] = eUserAction.YOffset;
            FFTDisplay.EventActions[eUserEvent.MouseDragYShift] = eUserAction.YOffset;

            /* when zooming in X direction in FFTDisplay, update waterfall view also */
            FFTDisplay.EventActions[eUserEvent.MouseWheelUpShift] = eUserAction.UserCallback;
            FFTDisplay.EventActions[eUserEvent.MouseWheelDownShift] = eUserAction.UserCallback;

            /* to force "show cursor" in other graph */
            AddUserEventCallback(eUserEvent.MouseEnter);
            AddUserEventCallback(eUserEvent.MouseLeave);

            /* when dragging, drag other view also */
            AddUserEventCallback(eUserEvent.MouseDragX);
            AddUserEventCallback(eUserEvent.MouseDragXShift);

            /* when zooming in? */
            //FFTDisplay.EventActions[eUserEvent.MouseWheelUp] = eUserAction.UserCallback;
            //FFTDisplay.EventActions[eUserEvent.MouseWheelDown] = eUserAction.UserCallback;

            /* configure update speed */
            //WaterfallDisplay.EventActions[eUserEvent.MouseWheelUpAlt] = eUserAction.UserCallback;
            //WaterfallDisplay.EventActions[eUserEvent.MouseWheelDownAlt] = eUserAction.UserCallback;
        }

        public string[] DisplayInformation
        {
            get
            {
                return FFTDisplay.DisplayInformation;
            }
        }

        public void AddUserEventCallback(eUserEvent evt)
        {
            FFTDisplay.EventActions[evt] = eUserAction.UserCallback;
            WaterfallDisplay.EventActions[evt] = eUserAction.UserCallback;
        }

        #region FFT Filter width display

        public string LimiterUpperDescription
        {
            get
            {
                return FFTDisplay.LimiterUpperDescription;
            }
            set
            {
                FFTDisplay.LimiterUpperDescription = value;
            }
        }

        public string LimiterLowerDescription
        {
            get
            {
                return FFTDisplay.LimiterLowerDescription;
            }
            set
            {
                FFTDisplay.LimiterLowerDescription = value;
            }
        }

        public bool LimiterDisplayEnabled
        {
            get
            {
                return FFTDisplay.LimiterDisplayEnabled;
            }
            set
            {
                FFTDisplay.LimiterDisplayEnabled = value;
            }
        }

        public double LimiterLowerLimit
        {
            get
            {
                return FFTDisplay.LimiterLowerLimit;
            }
            set
            {
                FFTDisplay.LimiterLowerLimit = value;
            }
        }

        public double LimiterUpperLimit
        {
            get
            {
                return FFTDisplay.LimiterUpperLimit;
            }
            set
            {
                FFTDisplay.LimiterUpperLimit = value;
            }
        }

        public Color LimiterColor
        {
            get
            {
                return FFTDisplay.LimiterColor;
            }
            set
            {
                FFTDisplay.LimiterColor = value;
            }
        }

        #endregion

        public bool DynamicLimits
        {
            get
            {
                return WaterfallDisplay.DynamicLimits;
            }
            set
            {
                WaterfallDisplay.DynamicLimits = value;
            }
        }


        /* return a value between -0.5 to 0.5 that indicates the cursor position relative to the center */
        public double RelativeCursorXPos
        {
            get
            {
                return FFTDisplay.XRelativeCoordFromCursorPos();
            }
        }

        public long FrequencyFromCursorPosOffset(double xOffset)
        {
            return FFTDisplay.FrequencyFromCursorPosOffset(xOffset);
        }

        public double FitSpectrumWidth = 1;

        public bool _FitSpectrumEnabled;
        public bool FitSpectrumEnabled
        {
            get
            {
                return _FitSpectrumEnabled;
            }
            set
            {
                _FitSpectrumEnabled = value;

                /* update sampling rates again */
                SamplingRate = SamplingRate;
            }
        }

        public double _SamplingRate;
        public double SamplingRate
        {
            set 
            {
                _SamplingRate = value;
                if (FitSpectrumEnabled)
                {
                    FFTDisplay.SamplingRate = value * FitSpectrumWidth;
                    WaterfallDisplay.SamplingRate = value * FitSpectrumWidth;
                }
                else
                {
                    FFTDisplay.SamplingRate = value;
                    WaterfallDisplay.SamplingRate = value;
                }
            }
            get { return _SamplingRate; }
        }

        public bool ChannelMode
        {
            get { return FFTDisplay.ChannelMode; }
            set
            {
                FFTDisplay.ChannelMode = value;
                WaterfallDisplay.ChannelMode = value;
            }
        }

        public FrequencyBand ChannelBandDetails
        {
            get { return FFTDisplay.ChannelBandDetails; }
            set
            {
                FFTDisplay.ChannelBandDetails = value;
                WaterfallDisplay.ChannelBandDetails = value;
            }
        }

        public double UpdateRate
        {
            set 
            { 
                FFTDisplay.UpdateRate = value;
                WaterfallDisplay.UpdateRate = value;
            }
            get { return FFTDisplay.UpdateRate; }
        }

        public long SamplesToAverage
        {
            set 
            {
                FFTDisplay.SamplesToAverage = value;
                WaterfallDisplay.SamplesToAverage = value;
            }
            get { return FFTDisplay.SamplesToAverage; }
        }

        public bool SavingEnabled
        {
            get { return WaterfallDisplay.SavingEnabled; }
            set { WaterfallDisplay.SavingEnabled = value; }
        }

        public string SavingName
        {
            get { return WaterfallDisplay.SavingName; }
            set { WaterfallDisplay.SavingName = value; }
        }

        public double VerticalSmooth
        {
            get { return FFTDisplay.VerticalSmooth; }
            set 
            { 
                FFTDisplay.VerticalSmooth = value;
                WaterfallDisplay.VerticalSmooth = value;
            }
        }

        public double CenterFrequency
        {
            get
            {
                return FFTDisplay.CenterFrequency;
            }
            set
            {
                FFTDisplay.CenterFrequency = value;
                WaterfallDisplay.CenterFrequency = value;
            }
        }


        public long Frequency
        {
            get
            {
                return FFTDisplay.FrequencyFromCursorPos();
            }
        }

        public FFTTransformer.eWindowingFunction WindowingFunction
        {
            get { return FFT.WindowingFunction; }
            set { FFT.WindowingFunction = value; }
        }

        public void UserEventCallbackFunc(eUserEvent evt, double param)
        {
            switch (evt)
            {
                case eUserEvent.MouseEnter:
                    FFTDisplay.ShowVerticalCursor = true;
                    WaterfallDisplay.ShowVerticalCursor = true;
                    break;

                case eUserEvent.MouseLeave:
                    FFTDisplay.ShowVerticalCursor = false;
                    WaterfallDisplay.ShowVerticalCursor = false;
                    break;

                /* used to paint waterfall level bars */
                case eUserEvent.StatusUpdated:
                    /* level bars toggled off */
                    if (!WaterfallDisplay.LevelBarActive && LevelBarActive)
                    {
                        FFTDisplay.LabelledHorLines.Remove(LevelBarLower);
                        if (!WaterfallDisplay.DynamicLimits)
                        {
                            FFTDisplay.LabelledHorLines.Remove(LevelBarUpper);
                        }
                        LevelBarActive = false;
                    }

                    /* level bars toggled on */
                    if (WaterfallDisplay.LevelBarActive && !LevelBarActive)
                    {
                        FFTDisplay.LabelledHorLines.AddLast(LevelBarLower);
                        if (!WaterfallDisplay.DynamicLimits)
                        {
                            FFTDisplay.LabelledHorLines.AddLast(LevelBarUpper);
                        }
                        LevelBarActive = true;
                    }

                    if (!WaterfallDisplay.DynamicLimits)
                    {
                        LevelBarLower.Position = WaterfallDisplay.LeveldBBlack;
                        LevelBarUpper.Position = WaterfallDisplay.LeveldBWhite;
                    }
                    else
                    {
                        LevelBarLower.Position = WaterfallDisplay.LeveldBBlack;
                    }
                    FFTDisplay.UpdateOverlays = true;
                    break;

                case eUserEvent.MouseWheelUp:
                    FFTDisplay.ProcessUserAction(eUserAction.YZoomIn, param);
                    break;

                case eUserEvent.MouseWheelDown:
                    FFTDisplay.ProcessUserAction(eUserAction.YZoomOut, param);
                    break;


                case eUserEvent.MousePosX:
                    FFTDisplay.ProcessUserAction(eUserAction.XPos, param);
                    WaterfallDisplay.ProcessUserAction(eUserAction.XPos, param);
                    break;

                case eUserEvent.MouseDragXShift:
                    FFTDisplay.ProcessUserAction(eUserAction.XOffsetOverview, param);
                    WaterfallDisplay.ProcessUserAction(eUserAction.XOffsetOverview, param);
                    break;

                case eUserEvent.MouseDragX:
                    FFTDisplay.ProcessUserAction(eUserAction.XOffset, param);
                    WaterfallDisplay.ProcessUserAction(eUserAction.XOffset, param);
                    break;

                case eUserEvent.MouseWheelUpShift:
                    FFTDisplay.ProcessUserAction(eUserAction.XZoomIn, param);
                    WaterfallDisplay.ProcessUserAction(eUserAction.XZoomIn, param);
                    break;

                case eUserEvent.MouseWheelDownShift:
                    FFTDisplay.ProcessUserAction(eUserAction.XZoomOut, param);
                    WaterfallDisplay.ProcessUserAction(eUserAction.XZoomOut, param);
                    break;

                    /*
                case eUserEvent.MouseWheelUpAlt:
                    if (WaterfallDisplay.UpdateRate < 512)
                    {
                        FFTDisplay.UpdateRate *= 2;
                        WaterfallDisplay.UpdateRate *= 2;
                    }
                    break;

                case eUserEvent.MouseWheelDownAlt:
                    if (WaterfallDisplay.UpdateRate > 1)
                    {
                        FFTDisplay.UpdateRate /= 2;
                        WaterfallDisplay.UpdateRate /= 2;
                    }
                    break;
                     * */
            }

            /* finally inform master form about the event */
            if (UserEventCallback != null)
            {
                UserEventCallback(evt, param);
            }
        }

        public int SpectParts
        {
            get { return FFTDisplay.SpectParts; }
            set
            {
                FFTDisplay.SpectParts = value;
                WaterfallDisplay.SpectParts = value;
            }
        }

        public int FFTSize
        {
            get { return _FFTSize; }
            set
            {
                lock (FFTLock)
                {
                    _FFTSize = value;
                    FFTResult = new double[_FFTSize];
                    FFT = new FFTTransformer(value);

                    FFTDisplay.FFTSize = value;
                    WaterfallDisplay.FFTSize = value;
                }
            }
        }

        public void FocusHovered()
        {
            if (WaterfallDisplay.MouseHovering)
            {
                WaterfallDisplay.Focus();
            }
            if (FFTDisplay.MouseHovering)
            {
                FFTDisplay.Focus();
            }
        }

        public void ProcessData(byte[] dataBuffer)
        {
            const int bytePerSample = 2;
            const int channels = 2;

            if (FFTDisplay.EnoughData)
                return;

            lock (FFTLock)
            {
                int samplePairs = dataBuffer.Length / (channels * bytePerSample);

                for (int samplePair = 0; samplePair < samplePairs; samplePair++)
                {
                    int samplePairPos = samplePair * bytePerSample * channels;
                    double I = ByteUtil.getDoubleFromBytes(dataBuffer, samplePairPos);
                    double Q = ByteUtil.getDoubleFromBytes(dataBuffer, samplePairPos + bytePerSample);


                    FFT.AddSample(I, Q);

                    if (FFT.ResultAvailable)
                    {
                        FFT.GetResultSquared(FFTResult);

                        FFTDisplay.ProcessFFTData(FFTResult);
                        WaterfallDisplay.ProcessFFTData(FFTResult);
                    }
                }
            }
        }

        public void ProcessData(double[] iSamples, double[] qSamples)
        {
            ProcessData(iSamples, qSamples, 0, 0);
        }

        public void ProcessData(double[] iSamples, double[] qSamples, int spectPart, double baseAmp)
        {
            if (FFTDisplay.EnoughData)
                return;

            lock (FFTLock)
            {
                int samplePairs = iSamples.Length;

                for (int samplePair = 0; samplePair < samplePairs; samplePair++)
                {
                    double I = iSamples[samplePair];
                    double Q = qSamples[samplePair];

                    FFT.AddSample(I, Q);

                    if (FFT.ResultAvailable)
                    {
                        FFT.GetResultSquared(FFTResult);

                        if (FitSpectrumEnabled)
                        {
                            CenterCut((int)(FitSpectrumWidth * FFTSize));
                            FFTDisplay.ProcessFFTData(FFTResultPartial, spectPart, baseAmp);
                            WaterfallDisplay.ProcessFFTData(FFTResultPartial, spectPart, baseAmp);
                        }
                        else
                        {
                            FFTDisplay.ProcessFFTData(FFTResult, spectPart, baseAmp);
                            WaterfallDisplay.ProcessFFTData(FFTResult, spectPart, baseAmp);
                        }
                    }
                }
            }
        }

        private void CenterCut(int FFTCenterWidth)
        {
            if (FFTResultPartial.Length !=  FFTCenterWidth)
            {
                FFTResultPartial = new double[FFTCenterWidth];
            }

            for (int pos = 0; pos < FFTCenterWidth; pos++)
            {
                FFTResultPartial[pos] = FFTResult[FFTResult.Length / 2 - FFTCenterWidth / 2 + pos];
            }
        }

        public void ProcessSample(double I, double Q)
        {
            ProcessSample(I, Q, 0, 1, 0);
        }

        public void ProcessSample(double I, double Q, int spectPart, int spectParts, double baseAmp)
        {
            lock (FFTLock)
            {
                FFT.AddSample(I, Q);

                if (FFT.ResultAvailable)
                {
                    FFT.GetResultSquared(FFTResult);

                    FFTDisplay.ProcessFFTData(FFTResult, spectPart, baseAmp);
                    WaterfallDisplay.ProcessFFTData(FFTResult, spectPart, baseAmp);
                }
            }
        }


        public LinkedList<FrequencyMarker> Markers
        {
            set
            {
                FFTDisplay.LabelledVertLines.Clear();

                foreach (FrequencyMarker marker in value)
                {
                    FFTDisplay.LabelledVertLines.AddLast(new LabelledLine(marker.Label, marker.Frequency, MarkerColor));
                }

                FFTDisplay.UpdateOverlays = true;
            }
        }
    }
}