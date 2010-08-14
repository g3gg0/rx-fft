using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibRXFFT.Libraries.SampleSources;
using LibRXFFT.Libraries.SignalProcessing;
using LibRXFFT.Libraries.ShmemChain;
using System.Threading;
using LibRXFFT.Libraries;
using RX_FFT.Components.GDI;

namespace RX_Radio
{
    public class AudioDemodulator
    {
        public ShmemSampleSource AudioInSampleSource;
        public SharedMem AudioOutShmem;

        protected Thread AudioThread;

        public bool AudioThreadRun = true;
        public Demodulation DemodOptions = new Demodulation();
        public long LastSamplingRate = 48000;
        public bool ProcessPaused = false;


        protected double[] DecimatedInputI = new double[1];
        protected double[] DecimatedInputQ = new double[1];
        protected double[] AudioSampleBuffer = new double[1];
        protected double[] AudioSampleBufferDecim = new double[1];

        public AudioDemodulator()
        {
        }

        public void Start()
        {
            AudioThread = new Thread(AudioReadFunc);
            AudioThread.Start();
        }

        public void Stop()
        {
            if (AudioThread != null)
            {
                AudioThread.Abort();
                AudioThread = null;
            }
        }

        void AudioReadFunc()
        {
            try
            {
                int lastAudioDecim = 1;
                int lastInputDecim = 1;
                bool lastCursorWinEnabled = false;

                double[] inputI;
                double[] inputQ;
                byte[] AudioOutBinary = null;

                AudioInSampleSource.SamplesPerBlock = 512;

                while (AudioThreadRun)
                {
                    lock (AudioInSampleSource)
                    {
                        if (AudioInSampleSource != null)
                        {
                            AudioInSampleSource.Read();

                            lock (AudioInSampleSource.SampleBufferLock)
                            {
                                inputI = AudioInSampleSource.SourceSamplesI;
                                inputQ = AudioInSampleSource.SourceSamplesQ;

                                lock (DemodOptions)
                                {
                                    bool blockSizeChanged = AudioSampleBuffer.Length != (inputI.Length / lastInputDecim);
                                    bool rateChanged = (DemodOptions.InputRate != AudioInSampleSource.OutputSamplingRate);
                                    lastCursorWinEnabled = DemodOptions.CursorPositionWindowEnabled;

                                    if (blockSizeChanged || DemodOptions.ReinitSound || rateChanged)
                                    {
                                        DemodOptions.InputRate = (long)AudioInSampleSource.OutputSamplingRate;
                                        DemodOptions.SoundDevice.SetInputRate((int)DemodOptions.AudioRate);
                                        DemodOptions.ReinitSound = false;

                                        lastAudioDecim = DemodOptions.AudioDecimation;
                                        lastInputDecim = DemodOptions.InputSignalDecimation;

                                        Array.Resize(ref AudioSampleBuffer, inputI.Length / lastInputDecim);
                                        Array.Resize(ref AudioSampleBufferDecim, inputI.Length / lastAudioDecim / lastInputDecim);

                                        Array.Resize(ref DecimatedInputI, inputI.Length / lastInputDecim);
                                        Array.Resize(ref DecimatedInputQ, inputI.Length / lastInputDecim);

                                        DemodOptions.UpdateListeners();
                                    }
                                }

                                if (!ProcessPaused)
                                {
                                    lock (DemodOptions)
                                    {
                                        if (DemodOptions.DemodulationEnabled)
                                        {
                                            if (lastCursorWinEnabled)
                                            {
                                                /* frequency translation */
                                                DemodOptions.DemodulationDownmixer.ProcessData(inputI, inputQ, inputI, inputQ);

                                                /* lowpass */

                                                // 43% (with 53% CPU load)
                                                DemodOptions.CursorWindowFilterThreadI.Process(inputI, inputI);
                                                DemodOptions.CursorWindowFilterThreadQ.Process(inputQ, inputQ);

                                                WaitHandle.WaitAll(DemodOptions.CursorWindowFilterEvents);

                                                /* decimate input signal */
                                                if (lastInputDecim > 1)
                                                {
                                                    for (int pos = 0; pos < DecimatedInputI.Length; pos++)
                                                    {
                                                        DecimatedInputI[pos] = inputI[pos * lastInputDecim];
                                                        DecimatedInputQ[pos] = inputQ[pos * lastInputDecim];
                                                    }

                                                    inputI = DecimatedInputI;
                                                    inputQ = DecimatedInputQ;
                                                }
                                            }

                                            /* in this block are some samples that can be demodulated. used for squelch */
                                            bool haveSamplesToDemodulate = true;

                                            /* squelch */
                                            if (DemodOptions.SquelchEnabled)
                                            {
                                                double totalStrength = 0;
                                                double maxStrength = 0;
                                                double limit = DBTools.SquaredSampleFromdB(DemodOptions.SquelchLowerLimit);

                                                /* default: nothing to demodulate */
                                                haveSamplesToDemodulate = false;

                                                for (int pos = 0; pos < inputI.Length; pos++)
                                                {
                                                    double strength = inputI[pos] * inputI[pos] + inputQ[pos] * inputQ[pos];

                                                    totalStrength += strength;
                                                    maxStrength = Math.Max(maxStrength, strength);

                                                    if (strength < limit)
                                                    {
                                                        /* below limit, close squelch? */
                                                        if (DemodOptions.SquelchState == Demodulation.eSquelchState.Open)
                                                        {
                                                            DemodOptions.SquelchSampleCounter++;
                                                            if (DemodOptions.SquelchSampleCounter > DemodOptions.SquelchSampleCount)
                                                            {
                                                                DemodOptions.SquelchSampleCounter = 0;
                                                                DemodOptions.SquelchState = Demodulation.eSquelchState.Closed;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            DemodOptions.SquelchSampleCounter = 0;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        /* over limit, open squelch? */
                                                        if (DemodOptions.SquelchState == Demodulation.eSquelchState.Closed)
                                                        {
                                                            DemodOptions.SquelchSampleCounter++;
                                                            if (DemodOptions.SquelchSampleCounter > DemodOptions.SquelchSampleCount)
                                                            {
                                                                DemodOptions.SquelchSampleCounter = 0;
                                                                DemodOptions.SquelchState = Demodulation.eSquelchState.Open;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            DemodOptions.SquelchSampleCounter = 0;
                                                        }
                                                    }

                                                    if (DemodOptions.SquelchState == Demodulation.eSquelchState.Closed)
                                                    {
                                                        inputI[pos] = 0;
                                                        inputQ[pos] = 0;
                                                    }
                                                    else
                                                    {
                                                        /* demodulate this block since there are some usable samples */
                                                        haveSamplesToDemodulate = true;
                                                    }
                                                }

                                                DemodOptions.SquelchAverage = DBTools.SquaredSampleTodB(totalStrength / inputI.Length);
                                                DemodOptions.SquelchMax = DBTools.SquaredSampleTodB(maxStrength);

                                                DemodOptions.UpdateListeners();
                                            }

                                            /* demodulate signal */
                                            if (haveSamplesToDemodulate)
                                            {
                                                DemodOptions.Demod.ProcessData(inputI, inputQ, AudioSampleBuffer);

                                                if (DemodOptions.AudioLowPassEnabled)
                                                {
                                                    DemodOptions.AudioLowPass.Process(AudioSampleBuffer, AudioSampleBuffer);
                                                }
                                            }
                                            else
                                            {
                                                Array.Clear(AudioSampleBuffer, 0, AudioSampleBuffer.Length);
                                            }

                                            /* audio decimation */
                                            if (lastAudioDecim > 1)
                                            {
                                                double ampl = 1;
                                                if (DemodOptions.AudioAmplificationEnabled)
                                                {
                                                    ampl = DemodOptions.AudioAmplification;
                                                }

                                                for (int pos = 0; pos < AudioSampleBufferDecim.Length; pos++)
                                                {
                                                    AudioSampleBufferDecim[pos] = ampl * AudioSampleBuffer[pos * lastAudioDecim];
                                                }

                                                DemodOptions.SoundDevice.Write(AudioSampleBufferDecim);

                                                /* shmem output of demodulated signal */
                                                if (AudioOutBinary == null || AudioOutBinary.Length != AudioSampleBufferDecim.Length * 4)
                                                {
                                                    AudioOutBinary = new byte[AudioSampleBufferDecim.Length * 4];
                                                }
                                                ByteUtil.SamplesToBinary(AudioOutBinary, AudioSampleBufferDecim, AudioSampleBufferDecim, ByteUtil.eSampleFormat.Direct16BitIQFixedPoint, false);
                                            }
                                            else
                                            {
                                                if (DemodOptions.AudioAmplificationEnabled)
                                                {
                                                    for (int pos = 0; pos < AudioSampleBuffer.Length; pos++)
                                                    {
                                                        AudioSampleBuffer[pos] *= DemodOptions.AudioAmplification;
                                                    }
                                                }
                                                DemodOptions.SoundDevice.Write(AudioSampleBuffer);

                                                /* shmem output of demodulated signal */
                                                if (AudioOutBinary == null || AudioOutBinary.Length != AudioSampleBuffer.Length * 4)
                                                {
                                                    AudioOutBinary = new byte[AudioSampleBuffer.Length * 4];
                                                }
                                                ByteUtil.SamplesToBinary(AudioOutBinary, AudioSampleBuffer, AudioSampleBuffer, ByteUtil.eSampleFormat.Direct16BitIQFixedPoint, false);
                                            }

                                            AudioOutShmem.Rate = (long)(DemodOptions.InputRate / lastAudioDecim);
                                            AudioOutShmem.Write(AudioOutBinary);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            Thread.Sleep(100);
                        }
                    }
                }
            }
            catch (ThreadAbortException e)
            {
            }
            catch (Exception e)
            {
                Log.AddMessage("Exception in Audio Thread: " + e.ToString());
            }
        }
    }
}
