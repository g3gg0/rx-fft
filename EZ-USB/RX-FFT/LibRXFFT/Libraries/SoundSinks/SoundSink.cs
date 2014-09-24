using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibRXFFT.Libraries.SignalProcessing;

namespace LibRXFFT.Libraries.SoundSinks
{
    public interface SoundSink
    {
        double SamplingRate { get; set; }
        string Status { get; }
        string Description { set; }
        DemodulationState.eSquelchState SquelchState { set; }

        void Start();
        void Stop();
        void Shutdown();

        void Process(double[] samples);
    }
}
