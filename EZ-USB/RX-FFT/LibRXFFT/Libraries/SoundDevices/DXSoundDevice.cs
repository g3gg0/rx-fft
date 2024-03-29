﻿using System;
using SlimDX.DirectSound;
using SlimDX.Multimedia;
using LibRXFFT.Libraries.SignalProcessing;
using System.Collections;

namespace LibRXFFT.Libraries.SoundDevices
{
    public class DXSoundDevice
    {
        private SecondarySoundBuffer Secondary;

        private int CurrentWritePosition = 0;
        private int BytesWritten = 0;
        private bool PlayingStarted = false;
        private bool Started = false;
        private int RealAudioSamplingRate = 0;
        private int InputSamplingRate = 48000;
        public int BufferSize = 0;
        private int PrebufferBytes = 0;

        public int BufferUsage
        {
            get
            {
                int bytesUsed = 0;

                if (Secondary != null)
                {
                    /* get the number of samples that we are able to write total */
                    bytesUsed = CurrentWritePosition - Secondary.CurrentPlayPosition;
                    if (bytesUsed < 0)
                    {
                        bytesUsed += BufferSize - 1;
                    }
                }
                return bytesUsed;
            }
        }
        private const double SecondsToBuffer = 0.1f;
        private SoundBufferDescription Desc;
        private DirectSound Device;

        private Resampler SignalResampler = new Resampler(1);
        private double[] ResampleIn = new double[0];
        private double[] ResampledOut = new double[0];
        private short[] ResampledShorts = new short[0];

        private string ErrorString = null;
        private IntPtr Handle;

        public int[] SamplingRates = { 192000, 96000, 48000, 44100, 22100, 11050, 8000 };

        public DXSoundDevice(IntPtr handle) : this(handle, Guid.Empty) { }

        public DXSoundDevice(IntPtr handle, Guid device)
        {
            Handle = handle;
            Desc = new SoundBufferDescription();
            Desc.Format = new WaveFormat();
            Desc.Format.BitsPerSample = 16;
            Desc.Format.BlockAlignment = 2;
            Desc.Format.Channels = 1;
            Desc.Format.FormatTag = WaveFormatTag.Pcm;

            SignalResampler = new Resampler(1);

            InitDevice(device);
        }

        public static DeviceInfo[] GetDevices()
        {
            ArrayList infos = new ArrayList();
            DeviceCollection coll = DirectSound.GetDevices();

            for (int i = 0; i < coll.Count; i++)
            {
                DeviceInfo info = new DeviceInfo();
                info.Name = coll[i].Description;
                info.Guid = coll[i].DriverGuid;
                infos.Add(info);
            }

            return (DeviceInfo[])infos.ToArray(typeof(DeviceInfo));
        }

        void InitDevice(Guid device)
        {
            Device = new DirectSound(device);
            Device.SetCooperativeLevel(Handle, CooperativeLevel.Priority);

            /* search for highest possible sampling rate */
            bool success = false;
            foreach (int rate in SamplingRates)
            {
                if (!success)
                {
                    success = SetOutputRate(rate);
                }
            }
        }

        public int Rate
        {
            get { return InputSamplingRate; }
            set { SetInputRate(value); }
        }

        public string Status
        {
            get
            {
                if (ErrorString != null)
                {
                    return ErrorString;
                }

                if (!Started)
                {
                    return "Stopped";
                }

                if (Secondary == null)
                {
                    return "Invalid Rate";
                }

                try
                {
                    if ((Secondary.Status & BufferStatus.Playing) == BufferStatus.Playing)
                    {
                        return "Playing";
                    }
                    else
                    {
                        return "Idle";
                    }
                }
                catch (Exception e)
                {
                    return e.ToString();
                }
            }
        }

        public bool SetInputRate(int rate)
        {
            lock (this)
            {
                if (rate != InputSamplingRate || SignalResampler == null)
                {
                    Stop();

                    /*
                    if (rate > RealAudioSamplingRate)
                    {
                        ErrorString = "Sampling rate too high";
                        return false;
                    }*/

                    InputSamplingRate = rate;

                    Start();
                }
            }
            return true;
        }

        public bool SetOutputRate(int rate)
        {
            lock (this)
            {
                Stop();

                RealAudioSamplingRate = rate;

                /* update sound buffer description */
                Desc.Format.SamplesPerSecond = RealAudioSamplingRate;
                Desc.Format.AverageBytesPerSecond = RealAudioSamplingRate * 2;
                Desc.SizeInBytes = (int)(Desc.Format.AverageBytesPerSecond * SecondsToBuffer);
                Desc.Flags = BufferFlags.GlobalFocus | BufferFlags.GetCurrentPosition2;

                BufferSize = Desc.SizeInBytes;
                PrebufferBytes = BufferSize / 2;

                return Start();
            }
        }


        public bool Start()
        {
            lock (this)
            {
                Stop();
                try
                {
                    Secondary = new SecondarySoundBuffer(Device, Desc);
                    Secondary.CurrentPlayPosition = 0;

                    /* clear first */
                    Secondary.Write(new byte[Desc.SizeInBytes], 0, LockFlags.EntireBuffer);

                    Started = true;
                    ErrorString = null;
                }
                catch (Exception e)
                {
                    RealAudioSamplingRate = 0;
                    return false;
                }
            }
            return true;
        }

        public void Stop()
        {
            lock (this)
            {
                if (Secondary != null)
                {
                    Secondary.Stop();
                    Secondary.Dispose();
                    Secondary = null;
                }

                BytesWritten = 0;
                CurrentWritePosition = 0;
                PlayingStarted = false;
                Started = false;
            }
        }


        public void Write(double[] data)
        {
            short[] buff = new short[data.Length];

            for (int pos = 0; pos < data.Length; pos++)
            {
                buff[pos] = (short)(data[pos] * short.MaxValue);
            }

            Write(buff);
        }

        public void Write(short[] sampleBuffer)
        {
            lock (this)
            {
                if (!Started)
                {
                    return;
                }

                if (Secondary == null || InputSamplingRate == 0 || RealAudioSamplingRate == 0)
                {
                    ErrorString = "Invalid sampling rate";
                    return;
                }

                try
                {
                    /* resample shorts in sampleBuffer */
                    if (InputSamplingRate != RealAudioSamplingRate)
                    {
                        if (ResampleIn.Length != sampleBuffer.Length)
                        {
                            Array.Resize(ref ResampleIn, sampleBuffer.Length);
                        }
                        for (int pos = 0; pos < sampleBuffer.Length; pos++)
                        {
                            ResampleIn[pos] = sampleBuffer[pos] / (double)short.MaxValue;
                        }

                        SignalResampler.Oversampling = (decimal)RealAudioSamplingRate / InputSamplingRate; 
                        SignalResampler.Resample(ResampleIn, ref ResampledOut);

                        if (ResampledShorts.Length != ResampledOut.Length)
                        {
                            Array.Resize(ref ResampledShorts, ResampledOut.Length);
                        }
                        for (int pos = 0; pos < ResampledShorts.Length; pos++)
                        {
                            ResampledShorts[pos] = (short)(ResampledOut[pos] * (double)short.MaxValue);
                        }

                        sampleBuffer = ResampledShorts;
                    }

                    int samplesToWrite = sampleBuffer.Length;
                    int bytesToWrite = samplesToWrite * Secondary.Format.BlockAlignment;
                    int currentPlayPosition = Secondary.CurrentPlayPosition;

                    /* get the number of samples that we are able to write total */
                    int bytesUsed = CurrentWritePosition - Secondary.CurrentWritePosition;
                    if (bytesUsed < 0)
                    {
                        bytesUsed += BufferSize;
                    }

                    /* keep one sample free */
                    int bytesFree = BufferSize - bytesUsed;
                    int samplesFree = bytesFree / Secondary.Format.BlockAlignment;

                    int maxSamples = Math.Min(samplesFree, samplesToWrite);
                    if (maxSamples == 0)
                    {
                        CurrentWritePosition = 0;
                        maxSamples = samplesToWrite;
                    }

                    if (Secondary.Status == BufferStatus.BufferLost)
                    {
                        Secondary.Restore();
                    }

                    /* can be written at once? or must be written in two steps? */
                    if (CurrentWritePosition + maxSamples > BufferSize)
                    {
                        int samplesFirst = BufferSize - CurrentWritePosition;
                        int samplesSecond = maxSamples - samplesFirst;

                        Secondary.Write(sampleBuffer, 0, samplesFirst, CurrentWritePosition, LockFlags.None);

                        /* write the second block at position 0 */
                        Secondary.Write(sampleBuffer, samplesFirst, samplesSecond, 0, LockFlags.None);
                    }
                    else
                    {
                        if (maxSamples > 0)
                        {
                            Secondary.Write(sampleBuffer, 0, maxSamples, CurrentWritePosition, LockFlags.None);
                        }
                    }

                    /* whole data was written, increment position */
                    CurrentWritePosition += bytesToWrite;
                    CurrentWritePosition %= BufferSize;

                    BytesWritten += bytesToWrite;

                    /* start playing when n packets were buffered */
                    if (!PlayingStarted && BytesWritten > PrebufferBytes)
                    {
                        PlayingStarted = true;
                        Secondary.CurrentPlayPosition = 0;
                        Secondary.Play(0, PlayFlags.Looping);
                    }

                    ErrorString = null;
                }
                catch (Exception e)
                {
                    ErrorString = "Processing Exception";
                    Console.Out.WriteLine("Exception: " + e.ToString());
                    Stop();
                    return;
                }
            }
        }
    }


    public class DeviceInfo
    {
        public string Name;
        public Guid Guid;

        public override string ToString()
        {
            return Name;
        }
    }
}