


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
        private string FileName = "";
        private FileStream FileStream = null;
        private Resampler AudioOversampler = null;

        private long NextFlushPosition = 0;
        private long FlushDelta = 1024 * 8;
        private Label StatusLabel = null;
        private Button SaveAsButton = null;

        private double SquelchClosedDelay = 0.5f;
        private int SquelchClosedCounter = 0;
        private bool Started = false;
        private eFileType FileType = eFileType.Unknown;

        public SoundFileSink(eFileType type, Control displayControl)
        {
            FileType = type;

            switch (FileType)
            {
                case eFileType.MP3:
                    FileName = "audio.mp3";
                    break;

                case eFileType.WAV:
                    FileName = "audio.wav";
                    break;
            }

            StatusLabel = new Label();
            StatusLabel.Text = FileName + ": Writer idle";
            //StatusLabel.Dock = DockStyle.Fill;

            SaveAsButton = new Button();
            SaveAsButton.Text = "Save As...";
            SaveAsButton.Click += (object sender, EventArgs e) => 
            {
                SaveFileDialog dlg = new SaveFileDialog();
                switch (FileType)
                {
                    case eFileType.MP3:
                        dlg.Filter = "MP3 File (*.mp3)|*.mp3|All files (*.*)|*.*";
                        break;

                    case eFileType.WAV:
                        dlg.Filter = "WAV File (*.wav)|*.wav|All files (*.*)|*.*";
                        break;
                }

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        try
                        {
                            File.Delete(dlg.FileName);
                        }
                        catch (Exception ex)
                        {
                        }

                        FileName = dlg.FileName;

                        if (Started)
                        {
                            Stop();
                            Start();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Could not create file: " + ex.GetType().ToString());
                    }
                }
            };
            Status = "";

            SplitContainer split = new SplitContainer();
            split.Orientation = System.Windows.Forms.Orientation.Horizontal;
            split.SplitterDistance = 60;
            split.Panel1.Controls.Add(StatusLabel);
            split.Panel2.Controls.Add(SaveAsButton);

            displayControl.Controls.Add(split);
        }

        #region SoundSink Member

        public double OutSamplingRate = 0;
        public double InSamplingRate = 0;
        public double SamplingRate
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

                    AudioOversampler = new Resampler((decimal)OutSamplingRate / (decimal)value);
                    AudioOversampler.Type = eResamplingType.SinC;
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
                        StatusLabel.Text = FileName + ": " + value;
                    }));
                }
                catch (Exception e)
                {
                }
            }
        }


        private void WriteHeader(FileStream OutFile)
        {
            /* Samples per second. */
            uint sample_rate = (uint)SamplingRate;
            /* Bytes per second */
            uint byte_sample_rate = sample_rate * 2;
            /* This is the size of the "fmt " subchunk */
            uint fmtsize = 16;
            /* WAV */
            ushort fmt = 1;
            /* Mono = 1 channel */
            ushort chans = 1;
            ushort block_align = 2;
            ushort bits_per_sample = 16;
            ushort extra_format = 320;
            /* This is the size of the "fact" subchunk */
            uint factsize = 4;
            /* Number of samples in the data chunk */
            uint num_samples = 0;
            /* Number of bytes in the data chunk */
            uint size = 0;
            /* Write a GSM header, ignoring sizes which will be filled in later */

            ASCIIEncoding enc = new ASCIIEncoding();

            OutFile.Seek(0, SeekOrigin.Begin);

            /*  0: Chunk ID */
            WriteBuffer(OutFile, enc.GetBytes("RIFF"));
            /*  4: Chunk Size */
            WriteBuffer(OutFile, BitConverter.GetBytes((uint)(OutFile.Length - 8)));
            /*  8: Chunk Format */
            WriteBuffer(OutFile, enc.GetBytes("WAVE"));
            /* 12: Subchunk 1: ID */
            WriteBuffer(OutFile, enc.GetBytes("fmt "));
            /* 16: Subchunk 1: Size (minus 8) */
            WriteBuffer(OutFile, BitConverter.GetBytes(fmtsize));
            /* 20: Subchunk 1: Audio format */
            WriteBuffer(OutFile, BitConverter.GetBytes(fmt));
            /* 22: Subchunk 1: Number of channels */
            WriteBuffer(OutFile, BitConverter.GetBytes(chans));
            /* 24: Subchunk 1: Sample rate */
            WriteBuffer(OutFile, BitConverter.GetBytes(sample_rate));
            /* 28: Subchunk 1: Byte rate */
            WriteBuffer(OutFile, BitConverter.GetBytes(byte_sample_rate));
            /* 32: Subchunk 1: Block align */
            WriteBuffer(OutFile, BitConverter.GetBytes(block_align));
            /* 36: Subchunk 1: Bits per sample */
            WriteBuffer(OutFile, BitConverter.GetBytes(bits_per_sample));

            /* 52: Subchunk 3: ID */
            WriteBuffer(OutFile, enc.GetBytes("data"));
            /* 56: Subchunk 3: Size */
            WriteBuffer(OutFile, BitConverter.GetBytes((uint)(OutFile.Length - (OutFile.Position + 4))));

            OutFile.Seek(0, SeekOrigin.End);
        }

        private void WriteBuffer(FileStream OutFile, byte[] data)
        {
            OutFile.Write(data, 0, data.Length);
        }

        public void Start()
        {
            try
            {
                lock (this)
                {
                    if (File.Exists(FileName))
                    {
                        FileStream = File.Open(FileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
                    }
                    else
                    {
                        FileStream = File.Open(FileName, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite);
                    }

                    switch (FileType)
                    {
                        case eFileType.MP3:
                            Mp3Writer = new Mp3Writer(new WaveFormat((int)OutSamplingRate, 16, 1));
                            break;

                        case eFileType.WAV:
                            WriteHeader(FileStream);
                            break;
                    }

                    NextFlushPosition = FlushDelta;

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
                    switch (FileType)
                    {
                        case eFileType.MP3:
                            break;

                        case eFileType.WAV:
                            WriteHeader(FileStream);
                            break;
                    }
                    FileStream.Close();
                    FileStream = null;
                }
                if (Mp3Writer != null)
                {
                    Mp3Writer.Close();
                    Mp3Writer = null;
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
            lock (this)
            {
                AudioOversampler.Resample(data, ref ProcessDoubleBuffer);
            }

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
                if (FileStream == null)
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

                switch (FileType)
                {
                    case eFileType.MP3:
                        if (Mp3Writer == null)
                        {
                            return;
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

                        break;

                    case eFileType.WAV:
                        FileStream.Write(data, 0, data.Length);
                        break;
                }

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
