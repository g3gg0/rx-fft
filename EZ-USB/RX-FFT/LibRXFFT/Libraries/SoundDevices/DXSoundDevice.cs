
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
using SlimDX.DirectSound;
using SlimDX.Multimedia;
using SlimDX;

public class DXSoundDevice
{
    private SoundManager Manager;
    private SecondarySoundBuffer Secondary;

    private int SampleBufferPos = 0;
    private int PacketsPlayed = 0;
    private int SamplingRate = 48000;
    private int BufferSize = 0;

    public DXSoundDevice(IntPtr Handle)
    {
        Manager = new SoundManager(Handle, CooperativeLevel.Priority);
        Manager.SetPrimaryBuffer(2, SamplingRate, 16);

        SetRate(SamplingRate);
    }

    public int Rate
    {
        get { return SamplingRate; }
        set { SetRate(value); }
    }

    public string Status
    {
        get
        {
            if (Secondary == null)
                return "Invalid Rate";

            try
            {
                if ((Secondary.Status & BufferStatus.Playing) == BufferStatus.Playing)
                    return "Playing";
                else
                    return "Idle";
            }
            catch (Exception e)
            {
                return e.ToString();
            }

        }
    }

    public bool SetRate(int rate)
    {
        bool running = false;

        if (Secondary != null)
        {
            running = (Secondary.Status & BufferStatus.Playing) == BufferStatus.Playing;
            Secondary.Stop();
            Secondary.Dispose();
            Secondary = null;
        }

        SamplingRate = rate;

        SoundBufferDescription description = new SoundBufferDescription();
        description.Format = new WaveFormat();
        description.Format.BitsPerSample = 16;
        description.Format.BlockAlignment = 2;
        description.Format.Channels = 1;
        description.Format.FormatTag = WaveFormatTag.Pcm;
        description.Format.SamplesPerSecond = SamplingRate;
        description.Format.AverageBytesPerSecond = SamplingRate * 2;
        description.SizeInBytes = description.Format.AverageBytesPerSecond / 4;
        description.Flags = BufferFlags.GlobalFocus | BufferFlags.GetCurrentPosition2;

        BufferSize = description.SizeInBytes;

        try
        {
            Secondary = new SecondarySoundBuffer(Manager.Device, description);
            if(running)
            {
                Secondary.CurrentPlayPosition = 0;
                Secondary.Play(0, PlayFlags.Looping);
            }
        }
        catch (Exception e)
        {
            SamplingRate = 0;
            return false;
        }

        return true;
    }


    public void Close()
    {
        if (Secondary != null)
        {
            Secondary.Stop();
            Secondary.Dispose();
        }
        Manager.Dispose();
    }

    public void Stop()
    {
        if (Secondary != null)
        {
            Secondary.Stop();
            Secondary.CurrentPlayPosition = 0;
        }
        SampleBufferPos = 0;
        PacketsPlayed = 0;
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

    public void Write(short[] data)
    {
        if (Secondary == null)
            return;

        try
        {
            if (Secondary.Status == BufferStatus.BufferLost)
                Secondary.Restore();

            int maxSamples = (BufferSize - SampleBufferPos) / Secondary.Format.BlockAlignment;

            if (maxSamples < data.Length)
            {
                short[] tmpBuff;

                tmpBuff = new short[maxSamples];
                Array.Copy(data, 0, tmpBuff, 0, tmpBuff.Length);
                Secondary.Write(tmpBuff, SampleBufferPos, LockFlags.None);

                tmpBuff = new short[data.Length - maxSamples];
                Array.Copy(data, maxSamples, tmpBuff, 0, tmpBuff.Length);
                Secondary.Write(tmpBuff, 0, LockFlags.None);
            }
            else
            {
                Secondary.Write(data, SampleBufferPos, LockFlags.None);
            }
            /*
            int sampleDistance = SampleBufferPos - Secondary.CurrentPlayPosition;
            if (sampleDistance < 0)
                sampleDistance += BufferSize;

            if (sampleDistance > data.Length * 2)
                PacketsPlayed = 0;
            */

            SampleBufferPos += data.Length * Secondary.Format.BlockAlignment;
            SampleBufferPos %= BufferSize;

            if (PacketsPlayed++ == 2)
            {
                Secondary.CurrentPlayPosition = 0;
                Secondary.Play(0, PlayFlags.Looping);
            }
        }
        catch (Exception e)
        {
            Console.Out.WriteLine("Exception: " + e.ToString());
            Secondary.Stop();
            PacketsPlayed = 0;
        }
    }

    public class SoundManager : IDisposable
    {
        private DirectSound device;

        public DirectSound Device
        {
            get { return device; }
        }

        public SoundManager(IntPtr Handle, CooperativeLevel level)
        {
            device = new DirectSound();
            device.SetCooperativeLevel(Handle, level);
        }

        public void SetPrimaryBuffer(short channels, int frequency, short bitRate)
        {
            SoundBufferDescription desc = new SoundBufferDescription();
            desc.Flags = BufferFlags.PrimaryBuffer;

            using (PrimarySoundBuffer primary = new PrimarySoundBuffer(device, desc))
            {
                WaveFormatExtensible format = new WaveFormatExtensible();
                format.FormatTag = WaveFormatTag.Pcm;
                format.Channels = channels;
                format.SamplesPerSecond = frequency;
                format.BitsPerSample = bitRate;
                format.BlockAlignment = (short)(bitRate / 8 * channels);
                format.AverageBytesPerSecond = frequency * format.BlockAlignment;

                primary.Format = format;
            }
        }

        public SoundListener3D Create3DListener()
        {
            SoundListener3D listener = null;
            SoundBufferDescription description = new SoundBufferDescription();
            description.Flags = BufferFlags.PrimaryBuffer | BufferFlags.Control3D;

            using (PrimarySoundBuffer buffer = new PrimarySoundBuffer(device, description))
            {
                listener = new SoundListener3D(buffer);
            }

            return listener;
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (device != null)
                device.Dispose();

            device = null;
        }

        #endregion
    }

}