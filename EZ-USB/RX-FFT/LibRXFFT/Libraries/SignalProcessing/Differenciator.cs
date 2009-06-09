namespace LibRXFFT.Libraries.SignalProcessing
{
    public class Differenciator
    {
        public static double[] Differenciate(double[] srcData)
        {
            return Differenciate(srcData, null);
        }

        public static double[] Differenciate(double[] srcData, double[] dstData)
        {
            if ( dstData == null)
                dstData = new double[srcData.Length];

            double lastVal = 0;

            for (int pos = 0; pos < srcData.Length; pos++ )
            {
                dstData[pos] = srcData[pos] - lastVal;
                lastVal = srcData[pos];
            }

            return dstData;
        }
    }
}
