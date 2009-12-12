
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

    private int CurrentWritePosition = 0;
    private int BytesWritten = 0;
    private bool PlayingStarted = false;
    private int SamplingRate = 48000;
    private int BufferSize = 0;
    private int PrebufferBytes = 0;

    private const double SecondsToBuffer = 2.0f;

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
        description.SizeInBytes = (int)(description.Format.AverageBytesPerSecond * SecondsToBuffer);
        description.Flags = BufferFlags.GlobalFocus | BufferFlags.GetCurrentPosition2;

        BufferSize = description.SizeInBytes;
        PrebufferBytes = BufferSize / 16;
        CurrentWritePosition = 0;

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
        CurrentWritePosition = 0;
        BytesWritten = 0;
        PlayingStarted = false;
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
        if (Secondary == null)
            return;


        try
        {
            int samplesToWrite = sampleBuffer.Length;
            int bytesToWrite = samplesToWrite * Secondary.Format.BlockAlignment;
            int currentPlayPosition = Secondary.CurrentPlayPosition;


            /* get the number of samples that we are able to write total */
            int bytesUsed = CurrentWritePosition - currentPlayPosition;
            if(bytesUsed < 0)
                bytesUsed += BufferSize;

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
                Secondary.Restore();

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
                Secondary.Write(sampleBuffer, 0, maxSamples, CurrentWritePosition, LockFlags.None);
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
        }
        catch (Exception e)
        {
            Console.Out.WriteLine("Exception: " + e.ToString());
            Secondary.Stop();
            BytesWritten = 0;
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