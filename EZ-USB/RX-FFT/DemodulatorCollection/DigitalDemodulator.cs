namespace DemodulatorCollection
{
    public interface DigitalDemodulator
    {
        double SamplingRate { get; set; }

        void Init();        
        void Process(double iValue, double qValue);
    }
}
