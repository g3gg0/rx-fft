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
using LibRXFFT.Libraries.ShmemChain;

namespace LibRXFFT.Libraries.SoundSinks
{
    public class SharedMemSink : SoundSink
    {
        private SharedMem Shmem = null;
        private string Description = "";

        private long NextFlushPosition = 0;
        private long FlushDelta = 1024 * 16;
        private Label StatusLabel = null;

        private double SquelchClosedDelay = 0.5f;
        private int SquelchClosedCounter = 0;

        public SharedMemSink(Control displayControl)
        {
            Shmem = new SharedMem(-2, 0, "Demodulator output", 8192);
            StatusLabel = new Label();
            StatusLabel.Text = "Writer idle";
            StatusLabel.Dock = DockStyle.Fill;
            Status = "";

            displayControl.Controls.Add(StatusLabel);
        }

        #region SoundSink Member


        private byte[] ProcessByteBuffer = new byte[0];
        private short[] ProcessShortBuffer = new short[0];

        public long SamplingRate
        {
            get;
            set;
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
                DisplayedStatus = "Active...";
                Status = "";
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
            Status = "";
            DisplayedStatus = "Inactive...";
        }

        public void Process(double[] data)
        {
            if (ProcessShortBuffer.Length != data.Length)
            {
                Array.Resize<short>(ref ProcessShortBuffer, data.Length);
            }

            for (int pos = 0; pos < data.Length; pos++)
            {
                ProcessShortBuffer[pos] = (short)(data[pos] * short.MaxValue);
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
            if (Shmem == null)
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

            Shmem.Write(data, 0, data.Length);
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
