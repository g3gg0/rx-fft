using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibRXFFT.Libraries.Timers;
using LibRXFFT.Libraries.SampleSources;
using LibRXFFT.Libraries.ShmemChain;
using LibRXFFT.Components.DirectX;
using LibRXFFT.Libraries.Demodulators;
using System.Threading;
using RX_FFT.Components.GDI;
using LibRXFFT.Libraries.Misc;

namespace LibRXFFT.Libraries.SignalProcessing
{
    public class AudioDemodulator
    {
        public ShmemSampleSource SampleSource;
        public Thread AudioThread;

        public bool AudioThreadRun = true;
        public bool DemodulationActive = true;
        public DemodulationState DemodState = new DemodulationState();
        public PerformanceEnvelope PerformanceCounters = new PerformanceEnvelope();
        public bool ProcessPaused = false;

        private double[] DecimatedInputI = new double[0];
        private double[] DecimatedInputQ = new double[0];
        private double[] AudioSampleBuffer = new double[0];
        private double[] AudioSampleBufferDecim = new double[0];

        public long InputSamplingRate = 0;

        public AudioDemodulator()
        {
        }

        public void Start(ShmemSampleSource sampleSource)
        {
            SampleSource = new ShmemSampleSource("RX-FFT Audio Decoder", sampleSource.ShmemChannel.SrcChan, 1, 0); ;
            SampleSource.InvertedSpectrum = sampleSource.InvertedSpectrum;

            AudioThreadRun = true;
            AudioThread = new Thread(AudioReadFunc);
            AudioThread.Name = "Audio Decoder Thread";
            AudioThread.Start();

            StartSinks();
        }

        public void Stop()
        {
            if (AudioThread != null)
            {
                if (!AudioThread.Join(100))
                {
                    AudioThread.Abort();
                }

                AudioThread = null;
            }

            StopSinks();

            if (SampleSource != null)
            {
                SampleSource.Close();
                SampleSource = null;
            }
        }

        public void Close()
        {
            Stop();
        }

        private void StartSinks()
        {
            lock (DemodState.SoundSinkInfos)
            {
                foreach (SoundSinkInfo info in DemodState.SoundSinkInfos)
                {
                    info.Sink.Start();
                }
            }
        }

        private void StopSinks()
        {
            lock (DemodState.SoundSinkInfos)
            {
                foreach (SoundSinkInfo info in DemodState.SoundSinkInfos)
                {
                    info.Sink.Stop();
                }
            }
        }

        private void SetSamplingRate(long rate)
        {
            lock (DemodState.SoundSinkInfos)
            {
                foreach (SoundSinkInfo info in DemodState.SoundSinkInfos)
                {
                    info.Sink.SamplingRate = rate;
                }
            }
        }

        private void Process(double[] samples)
        {
            lock (DemodState.SoundSinkInfos)
            {
                foreach (SoundSinkInfo info in DemodState.SoundSinkInfos)
                {
                    info.Sink.Process(samples);
                }
            }
        }

        private void UpdateDemodInformation()
        {
            lock (DemodState.SoundSinkInfos)
            {
                foreach (SoundSinkInfo info in DemodState.SoundSinkInfos)
                {
                    string desc = "";

                    if (DemodState.Description != null)
                    {
                        desc = DemodState.Description;
                    }
                    else
                    {
                        desc = FrequencyFormatter.FreqToStringAccurate(DemodState.DemodulationFrequency);
                    }
                    desc += " (" + FrequencyFormatter.FreqToStringAccurate(DemodState.InputRate) + ")";


                    if (DemodState.SquelchEnabled)
                    {
                        if (DemodState.SquelchState == DemodulationState.eSquelchState.Closed)
                        {
                            desc += "SQ: OPEN";
                        }
                        else
                        {
                            desc += "SQ: idle";
                        }
                    }

                    info.Sink.Description = desc;

                    /* update squelch state */
                    if (DemodState.SquelchEnabled)
                    {
                        info.Sink.SquelchState = DemodState.SquelchState;
                    }
                    else
                    {
                        info.Sink.SquelchState = DemodulationState.eSquelchState.Open;
                    }
                }
            }
        }


        public void UpdateXLat()
        {
            /* calculate downmix frequency */
            double offset = DemodState.BaseFrequency - DemodState.DemodulationFrequency;
            double relative = offset / InputSamplingRate;

            if (DemodState.SignalDemodulator is SSBDemodulator)
            {
                switch (((SSBDemodulator)DemodState.SignalDemodulator).Type)
                {
                    case eSsbType.Lsb:
                        relative -= -0.5f / DemodState.CursorWindowFilterWidthFract;
                        break;
                    case eSsbType.Usb:
                        relative -= 0.5f / DemodState.CursorWindowFilterWidthFract;
                        break;
                }
            }

            bool valid = false;

            /* only if within the filter window */
            if (Math.Abs(relative) <= 0.5f)
            {
                DemodState.DemodulationDownmixer.TimeStep = relative * (2 * Math.PI);
                valid = true;
            }

            /* did something change? */
            if (DemodState.DemodulationPossible != valid)
            {
                /* possible to decode? start sinks again. */
                if (valid)
                {
                    StartSinks();
                }
                else
                {
                    StopSinks();
                }

                DemodState.DemodulationPossible = valid;
            }

            /*
            if (DemodState.Dialog != null)
            {
                if (valid)
                {
                    DemodState.Dialog.Frequency = DemodState.DemodulationFrequency;
                }
                DemodState.Dialog.UpdateInformation();
            }
            */
        }

        void AudioReadFunc()
        {
            try
            {
                long baseFreq = 0;
                long demodFreq = 0;
                long rate = 0;
                int lastAudioDecim = 1;
                int lastInputDecim = 1;
                bool lastCursorWinEnabled = false;
                bool lastSquelchEnabled = !DemodState.SquelchEnabled;

                double[] inputI;
                double[] inputQ;
                byte[] AudioOutBinary = null;

                SampleSource.SamplesPerBlock = 512;

                PerformanceCounters.Reset();
                PerformanceCounters.CounterRuntime.Start();

                //AudioShmem.TraceReads = true;

                while (AudioThreadRun)
                {
                    PerformanceCounters.CounterRuntime.Update();

                    if (DemodState.DemodulationEnabled)
                    {
                        lock (SampleSource)
                        {
                            PerformanceCounters.CounterReading.Start();
                            bool readSuccess = SampleSource.Read();
                            PerformanceCounters.CounterReading.Stop();

                            if (readSuccess && SampleSource.SamplesRead == SampleSource.SourceSamplesI.Length)
                            {
                                InputSamplingRate = (long)SampleSource.OutputSamplingRate;

                                PerformanceCounters.CounterProcessing.Start();
                                lock (SampleSource.SampleBufferLock)
                                {
                                    inputI = SampleSource.SourceSamplesI;
                                    inputQ = SampleSource.SourceSamplesQ;

                                    lock (DemodState)
                                    {
                                        if (DemodState.DemodView != null)
                                        {
                                            DemodState.DemodView.ProcessData(inputI, inputQ, DemodFFTView.eDataType.Input);
                                        }

                                        bool blockSizeChanged = AudioSampleBuffer.Length != (inputI.Length / lastInputDecim);
                                        bool rateChanged = Math.Abs(rate - InputSamplingRate) > 0;
                                        lastCursorWinEnabled = DemodState.CursorPositionWindowEnabled;

                                        if (blockSizeChanged || DemodState.ReinitSound || rateChanged || rate == 0)
                                        {
                                            rate = InputSamplingRate;

                                            DemodState.DemodulationDownmixer.SamplingRate = rate;
                                            DemodState.ReinitSound = false;

                                            UpdateXLat();
                                            UpdateDemodInformation();

                                            // // SND DemodState.SoundDevice.SetInputRate((int)DemodState.AudioRate);

                                            lastAudioDecim = DemodState.AudioDecimation;
                                            lastInputDecim = DemodState.InputSignalDecimation;

                                            Array.Resize<double>(ref AudioSampleBuffer, inputI.Length / lastInputDecim);
                                            Array.Resize<double>(ref AudioSampleBufferDecim, inputI.Length / lastAudioDecim / lastInputDecim);

                                            Array.Resize<double>(ref DecimatedInputI, inputI.Length / lastInputDecim);
                                            Array.Resize<double>(ref DecimatedInputQ, inputQ.Length / lastInputDecim);
                                        }
                                        SetSamplingRate((long)DemodState.AudioRate);

                                        /* check if input frequency or demodulation frequency has changed */
                                        if (baseFreq != DemodState.BaseFrequency || demodFreq != DemodState.DemodulationFrequency)
                                        {
                                            baseFreq = DemodState.BaseFrequency;
                                            demodFreq = DemodState.DemodulationFrequency;

                                            UpdateXLat();
                                            UpdateDemodInformation();
                                        }
                                    }

                                    if (!ProcessPaused)
                                    {
                                        lock (DemodState)
                                        {
                                            if (DemodState.DemodulationEnabled && DemodState.DemodulationPossible)
                                            {
                                                if (lastCursorWinEnabled)
                                                {
                                                    /* frequency translation */
                                                    PerformanceCounters.CounterXlat.Start();
                                                    DemodState.DemodulationDownmixer.ProcessData(inputI, inputQ, inputI, inputQ);
                                                    PerformanceCounters.CounterXlat.Stop();

                                                    if (DemodState.DemodView != null)
                                                    {
                                                        DemodState.DemodView.SamplingRate = rate;
                                                        DemodState.DemodView.ProcessData(inputI, inputQ, DemodFFTView.eDataType.Translated);
                                                    }

                                                    /* lowpass */
                                                    PerformanceCounters.CounterXlatLowpass.Start();

                                                    DemodState.CursorWindowFilterThreadI.Process(inputI, inputI);
                                                    DemodState.CursorWindowFilterThreadQ.Process(inputQ, inputQ);
                                                    WaitHandle.WaitAll(DemodState.CursorWindowFilterEvents);

                                                    PerformanceCounters.CounterXlatLowpass.Stop();

                                                    /* now translate again if its SSB demodulation */
                                                    if (DemodState.SignalDemodulator is SSBDemodulator)
                                                    {
                                                        DemodState.SSBDownmixer.ProcessData(inputI, inputQ, inputI, inputQ);
                                                    }

                                                    /* live view of demodulation products */
                                                    if (DemodState.DemodView != null)
                                                    {
                                                        DemodState.DemodView.ProcessData(inputI, inputQ, DemodFFTView.eDataType.Filtered);
                                                    }

                                                    /* decimate input signal */
                                                    PerformanceCounters.CounterXlatDecimate.Start();

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

                                                    PerformanceCounters.CounterXlatDecimate.Stop();
                                                    if (DemodState.DemodView != null)
                                                    {
                                                        DemodState.DemodView.ProcessData(inputI, inputQ, DemodFFTView.eDataType.Decimated);
                                                    }
                                                }

                                                /* in this block are some samples that can be demodulated. used for squelch */
                                                bool haveSamplesToDemodulate = true;

                                                /* squelch */
                                                if (lastSquelchEnabled != DemodState.SquelchEnabled)
                                                {
                                                    lastSquelchEnabled = DemodState.SquelchEnabled;
                                                    UpdateDemodInformation();
                                                }

                                                if (DemodState.SquelchEnabled)
                                                {
                                                    double totalStrength = 0;
                                                    double maxStrength = 0;
                                                    double limit = DBTools.SquaredSampleFromdB(DemodState.SquelchLowerLimit);

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
                                                            if (DemodState.SquelchState == DemodulationState.eSquelchState.Open)
                                                            {
                                                                DemodState.SquelchSampleCounter++;
                                                                if (DemodState.SquelchSampleCounter > DemodState.SquelchSampleCount)
                                                                {
                                                                    DemodState.SquelchSampleCounter = 0;
                                                                    DemodState.SquelchState = DemodulationState.eSquelchState.Closed;
                                                                    UpdateDemodInformation();
                                                                }
                                                            }
                                                            else
                                                            {
                                                                DemodState.SquelchSampleCounter = 0;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            /* over limit, open squelch? */
                                                            if (DemodState.SquelchState == DemodulationState.eSquelchState.Closed)
                                                            {
                                                                DemodState.SquelchSampleCounter++;
                                                                if (DemodState.SquelchSampleCounter > DemodState.SquelchSampleCount)
                                                                {
                                                                    DemodState.SquelchSampleCounter = 0;
                                                                    DemodState.SquelchState = DemodulationState.eSquelchState.Open;
                                                                    UpdateDemodInformation();
                                                                }
                                                            }
                                                            else
                                                            {
                                                                DemodState.SquelchSampleCounter = 0;
                                                            }
                                                        }

                                                        if (DemodState.SquelchState == DemodulationState.eSquelchState.Closed)
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

                                                    DemodState.SquelchAverage = DBTools.SquaredSampleTodB(totalStrength / inputI.Length);
                                                    DemodState.SquelchMax = DBTools.SquaredSampleTodB(maxStrength);

                                                    DemodState.UpdateListeners();
                                                }

                                                /* demodulate signal */
                                                if (haveSamplesToDemodulate)
                                                {
                                                    PerformanceCounters.CounterDemod.Start();
                                                    DemodState.SignalDemodulator.ProcessData(inputI, inputQ, AudioSampleBuffer);
                                                    PerformanceCounters.CounterDemod.Stop();

                                                    if (DemodState.AudioLowPassEnabled)
                                                    {
                                                        PerformanceCounters.CounterDemodLowpass.Start();
                                                        DemodState.AudioLowPass.Process(AudioSampleBuffer, AudioSampleBuffer);
                                                        PerformanceCounters.CounterDemodLowpass.Stop();
                                                    }
                                                }
                                                else
                                                {
                                                    Array.Clear(AudioSampleBuffer, 0, AudioSampleBuffer.Length);
                                                }

                                                /* audio decimation */
                                                if (lastAudioDecim > 1)
                                                {
                                                    PerformanceCounters.CounterDemodDecimate.Start();
                                                    double ampl = 1;
                                                    if (DemodState.AudioAmplificationEnabled)
                                                    {
                                                        ampl = DemodState.AudioAmplification;
                                                    }

                                                    for (int pos = 0; pos < AudioSampleBufferDecim.Length; pos++)
                                                    {
                                                        AudioSampleBufferDecim[pos] = ampl * AudioSampleBuffer[pos * lastAudioDecim];
                                                    }
                                                    PerformanceCounters.CounterDemodDecimate.Stop();

                                                    Process(AudioSampleBufferDecim);
                                                    // // SND DemodState.SoundDevice.Write(AudioSampleBufferDecim);

                                                    /* shmem output of demodulated signal */
                                                    if (AudioOutBinary == null || AudioOutBinary.Length != AudioSampleBufferDecim.Length * 4)
                                                    {
                                                        AudioOutBinary = new byte[AudioSampleBufferDecim.Length * 4];
                                                    }
                                                    ByteUtil.SamplesToBinary(AudioOutBinary, AudioSampleBufferDecim, AudioSampleBufferDecim, ByteUtil.eSampleFormat.Direct16BitIQFixedPoint, false);
                                                }
                                                else
                                                {
                                                    if (DemodState.AudioAmplificationEnabled)
                                                    {
                                                        for (int pos = 0; pos < AudioSampleBuffer.Length; pos++)
                                                        {
                                                            AudioSampleBuffer[pos] *= DemodState.AudioAmplification;
                                                        }
                                                    }

                                                    Process(AudioSampleBuffer);

                                                    /* shmem output of demodulated signal */
                                                    if (AudioOutBinary == null || AudioOutBinary.Length != AudioSampleBuffer.Length * 4)
                                                    {
                                                        AudioOutBinary = new byte[AudioSampleBuffer.Length * 4];
                                                    }
                                                    ByteUtil.SamplesToBinary(AudioOutBinary, AudioSampleBuffer, AudioSampleBuffer, ByteUtil.eSampleFormat.Direct16BitIQFixedPoint, false);
                                                }

                                                //AudioOutShmem.Rate = (long)(rate / lastAudioDecim);
                                                //AudioOutShmem.Write(AudioOutBinary);
                                            }
                                        }
                                        PerformanceCounters.CounterProcessing.Stop();

                                    }
                                    else
                                    {
                                        PerformanceCounters.CounterProcessing.Stop();
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
                /*
                if (DemodDialog != null)
                    DemodDialog.UpdateInformation();
                */
                PerformanceCounters.CounterRuntime.Stop();
            }
            catch (ThreadAbortException e)
            {
            }
            catch (Exception e)
            {
                Log.AddMessage("Exception in Audio Thread: " + e.ToString());
            }
        }


        public class PerformanceEnvelope
        {
            public HighPerformanceCounter CounterRuntime = new HighPerformanceCounter("Runtime");
            public HighPerformanceCounter CounterReading = new HighPerformanceCounter("Reading");
            public HighPerformanceCounter CounterProcessing = new HighPerformanceCounter("Processing");
            public HighPerformanceCounter CounterXlat = new HighPerformanceCounter("Translation");
            public HighPerformanceCounter CounterXlatLowpass = new HighPerformanceCounter("Translation LowPass");
            public HighPerformanceCounter CounterXlatDecimate = new HighPerformanceCounter("Translation Decim");
            public HighPerformanceCounter CounterDemod = new HighPerformanceCounter("Demodulation");
            public HighPerformanceCounter CounterDemodLowpass = new HighPerformanceCounter("Demodulation LowPass");
            public HighPerformanceCounter CounterDemodDecimate = new HighPerformanceCounter("Demodulation Decim");
            public HighPerformanceCounter CounterVisualization = new HighPerformanceCounter("Visualization");

            public void Reset()
            {
                CounterRuntime.Reset();
                CounterReading.Reset();
                CounterProcessing.Reset();
                CounterXlat.Reset();
                CounterXlatLowpass.Reset();
                CounterXlatDecimate.Reset();
                CounterDemod.Reset();
                CounterDemodLowpass.Reset();
                CounterDemodDecimate.Reset();
                CounterVisualization.Reset();
            }
        }

    }
}
