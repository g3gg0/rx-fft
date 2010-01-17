namespace LibRXFFT.Libraries.Filters
{
    public abstract class Filter
    {
        public abstract double[] Process(double[] inData, double[] outData);
        public abstract void Dispose();
    }
}
