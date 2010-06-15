using System;
using SlimDX.DirectSound;
using SlimDX.Multimedia;
using LibRXFFT.Libraries.SignalProcessing;

public class DXSoundDevice
{
    private SecondarySoundBuffer Secondary;

    private int CurrentWritePosition = 0;
    private int BytesWritten = 0;
    private bool PlayingStarted = false;
    private bool Started = false;
    private int SamplingRate = 48000;
    private int InputSamplingRate = 48000;
    private int BufferSize = 0;
    private int PrebufferBytes = 0;
    
    private const double SecondsToBuffer = 0.5f;
    private SoundBufferDescription Desc;
    private DirectSound Device;

    private Oversampler SignalOversampler;
    private double[] OversampleIn;
    private double[] OversampleOut;
    private short[] OversampledSamples;

    private string ErrorString = null;

    public int[] SamplingRates = { 192000, 96000, 48000, 44100, 22100, 11050 };

    public DXSoundDevice(IntPtr Handle)
    {
        Desc = new SoundBufferDescription();
        Desc.Format = new WaveFormat();
        Desc.Format.BitsPerSample = 16;
        Desc.Format.BlockAlignment = 2;
        Desc.Format.Channels = 1;
        Desc.Format.FormatTag = WaveFormatTag.Pcm;

        Device = new DirectSound();
        Device.SetCooperativeLevel(Handle, CooperativeLevel.Priority);

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
        get { return SamplingRate; }
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
            Stop();

            if (rate > SamplingRate)
            {
                ErrorString = "Sampling rate too high";
                return false;
            }

            InputSamplingRate = rate;
            SignalOversampler = new Oversampler((double)SamplingRate / (double)rate);
            SignalOversampler.Type = eOversamplingType.SinX;

            Start();
        }
        return true;
    }

    public bool SetOutputRate(int rate)
    {
        lock (this)
        {
            Stop();

            SamplingRate = rate;

            /* update sound buffer description */
            Desc.Format.SamplesPerSecond = SamplingRate;
            Desc.Format.AverageBytesPerSecond = SamplingRate * 2;
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
                SamplingRate = 0;
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

            if (Secondary == null || InputSamplingRate == 0 || SamplingRate == 0)
            {
                ErrorString = "Invalid sampling rate";
                return;
            }

            try
            {
                if (InputSamplingRate != SamplingRate)
                {
                    if (OversampleIn == null || OversampleIn.Length != sampleBuffer.Length)
                    {
                        OversampleIn = new double[sampleBuffer.Length];
                    }
                    for (int pos = 0; pos < sampleBuffer.Length; pos++)
                    {
                        OversampleIn[pos] = sampleBuffer[pos] / (double)short.MaxValue;
                    }
                    OversampleOut = SignalOversampler.Oversample(OversampleIn, OversampleOut);

                    if (OversampledSamples == null || OversampledSamples.Length != OversampleOut.Length)
                    {
                        OversampledSamples = new short[OversampleOut.Length];
                    }
                    for (int pos = 0; pos < OversampleOut.Length; pos++)
                    {
                        OversampledSamples[pos] = (short)(OversampleOut[pos] * (double)short.MaxValue);
                    }

                    sampleBuffer = OversampledSamples;
                }

                int samplesToWrite = sampleBuffer.Length;
                int bytesToWrite = samplesToWrite * Secondary.Format.BlockAlignment;
                int currentPlayPosition = Secondary.CurrentPlayPosition;


                /* get the number of samples that we are able to write total */
                int bytesUsed = CurrentWritePosition - currentPlayPosition;
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