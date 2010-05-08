
namespace DemodulatorCollection.Interfaces
{
    public interface DigitalDemodulator
    {
        BitClockSink BitSink { get; set; }
        double SamplingRate { get; set; }

        void Init();        
        void Process(double iValue, double qValue);
    }
}
