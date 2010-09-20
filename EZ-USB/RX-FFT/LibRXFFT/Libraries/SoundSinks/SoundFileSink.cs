


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using LibRXFFT.Libraries.SoundDevices;
using LibRXFFT.Components.GDI;
using System.Net.Sockets;
using System.Net;
using System.IO;
using LibRXFFT.Libraries.SampleSources;
using LibRXFFT.Libraries.SignalProcessing;

namespace LibRXFFT.Libraries.SoundSinks
{
    public enum eFileType
    {
        Unknown,
        MP3,
        WAV
    }

    public class SoundFileSink : SoundSink
    {
        private Mp3Writer Mp3Writer = null;
        private string Description = "";
        private string FileName = "d:\\audio.mp3";
        private Stream FileStream = null;
        private Oversampler AudioOversampler = null;

        private long NextFlushPosition = 0;
        private long FlushDelta = 1024 * 8;
        private Label StatusLabel = null;

        private double SquelchClosedDelay = 0.5f;
        private int SquelchClosedCounter = 0;
        private bool Started = false;

        public SoundFileSink(Control displayControl)
        {
            StatusLabel = new Label();
            StatusLabel.Text = "Writer idle";
            StatusLabel.Dock = DockStyle.Fill;
            Status = "";

            displayControl.Controls.Add(StatusLabel);
        }

        #region SoundSink Member

        public long OutSamplingRate = 0;
        public long InSamplingRate = 0;
        public long SamplingRate
        {
            get
            {
                return OutSamplingRate;
            }
            set
            {
                lock (this)
                {
                    /* dont change anything if nothing has changed */
                    if (value == InSamplingRate)
                    {
                        return;
                    }

                    bool oldState = Started;

                    Stop();
                    InSamplingRate = value;

                    if ((48000 % value) == 0 || (value % 48000) == 0)
                    {
                        /* a fract/multiple of 48kHz */
                        OutSamplingRate = 48000;
                    }
                    else if ((44100 % value) == 0 || (value % 44100) == 0)
                    {
                        /* a fract/multiple of 44,1kHz */
                        OutSamplingRate = 44100;
                    }
                    else
                    {
                        /* ToDo: not sure what to do :( */
                        OutSamplingRate = 48000;
                    }

                    AudioOversampler = new Oversampler((float)OutSamplingRate / value);
                    AudioOversampler.Type = eOversamplingType.SinC;
                    AudioOversampler.SinCDepth = 4;

                    /* start again if it was active before */
                    if (oldState)
                    {
                        Start();
                    }
                }
            }
        }

        public DemodulationState.eSquelchState SquelchState
        {
            get;
            set;
        }

        private string DisplayedStatus
        {
            set
            {
                try
                {
                    StatusLabel.BeginInvoke(new Action(() =>
                    {
                        StatusLabel.Text = value;
                    }));
                }
                catch (Exception e)
                {
                }
            }
        }

        public void Start()
        {
            try
            {
                lock (this)
                {
                    if (FileStream == null)
                    {
                        Mp3Writer = new Mp3Writer(new WaveFormat((int)OutSamplingRate, 16, 1));

                        if (File.Exists(FileName))
                        {
                            FileStream = File.Open(FileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                        }
                        else
                        {
                            FileStream = File.Open(FileName, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite);
                        }
                        NextFlushPosition = FlushDelta;
                    }

                    Started = true;
                    DisplayedStatus = "File opened...";
                    Status = "";
                }
            }
            catch (Exception e)
            {
                Status = "(fail)";
                DisplayedStatus = "Failed (" + e.GetType().ToString() + ")";
                return;
            }
        }

        public void Stop()
        {
            lock (this)
            {
                if (FileStream != null)
                {
                    FileStream.Close();
                    FileStream = null;
                }
                Started = false;
                Status = "";
                DisplayedStatus = "File closed...";
            }
        }

        private byte[] ProcessByteBuffer = new byte[0];
        private short[] ProcessShortBuffer = new short[0];
        private double[] ProcessDoubleBuffer = new double[0];

        public void Process(double[] data)
        {
            ProcessDoubleBuffer = AudioOversampler.Oversample(data, ProcessDoubleBuffer);

            if (ProcessShortBuffer.Length != ProcessDoubleBuffer.Length)
            {
                Array.Resize<short>(ref ProcessShortBuffer, ProcessDoubleBuffer.Length);
            }

            for (int pos = 0; pos < ProcessDoubleBuffer.Length; pos++)
            {
                ProcessShortBuffer[pos] = (short)(ProcessDoubleBuffer[pos] * short.MaxValue);
            }

            Process(ProcessShortBuffer);
        }


        public void Process(short[] data)
        {
            if (ProcessByteBuffer.Length != data.Length * 2)
            {
                Array.Resize<byte>(ref ProcessByteBuffer, data.Length * 2);
            }

            for (int pos = 0; pos < data.Length; pos++)
            {
                ProcessByteBuffer[2 * pos] = (byte)(data[pos] & 0xFF);
                ProcessByteBuffer[2 * pos + 1] = (byte)(data[pos] >> 8);
            }

            Process(ProcessByteBuffer);
        }

        public void Process(byte[] data)
        {
            lock (this)
            {
                if (Mp3Writer == null || FileStream == null)
                {
                    return;
                }

                if (SquelchState == DemodulationState.eSquelchState.Closed)
                {
                    if (SquelchClosedCounter >= SquelchClosedDelay * SamplingRate)
                    {
                        return;
                    }
                    else
                    {
                        SquelchClosedCounter += data.Length / 2;
                        Array.Clear(data, 0, data.Length);
                    }
                }
                else
                {
                    SquelchClosedCounter = 0;
                }

                /* feed samples into mp3 converter */
                Mp3Writer.Write(data);

                if (Mp3Writer.DataAvailable == 0)
                {
                    return;
                }

                /* write mp3 data */
                FileStream.Write(Mp3Writer.m_OutBuffer, 0, Mp3Writer.DataAvailable);
                Mp3Writer.DataAvailable = 0;

                /* flush every block */
                if (FileStream.Position > NextFlushPosition)
                {
                    DisplayedStatus = "Writing, Position: " + FileStream.Position;
                    NextFlushPosition = FileStream.Position + FlushDelta;
                    FileStream.Flush();
                }
            }
        }


        public string Status
        {
            get;
            set;
        }

        string SoundSink.Description
        {
            set
            {
                Description = value;
            }
        }

        public void Shutdown()
        {
        }

        #endregion
    }
}
