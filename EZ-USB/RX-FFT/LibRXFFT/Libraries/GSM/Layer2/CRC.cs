using System;

namespace LibRXFFT.Libraries.GSM.Layer2
{
    public static class CRC
    {
        public static readonly bool[] PolynomialTCHFR = new bool[]
                                                          {
                                                              true, false, true, true
                                                          };

        public static readonly bool[] PolynomialSCH = new bool[]
                                                          {
                                                              true, false, true, false, true, true, true, false, true,
                                                              false, true
                                                          };

        public static readonly bool[] PolynomialFIRE = new bool[]
                                                           {
                                                               true, false, false, false, false, false, false, false, false,
                                                               false, false, false, false, false, true, false, false,
                                                               true, false, false, false, false, false, true, false, false,
                                                               false, false, false, false, false, false, false, false,
                                                               false, false, false, true, false, false, true
                                                           };

        private static void DivMod2(bool[] data, bool[] polynomial)
        {
            if (!data[0])
                return;

            for (int pos = 0; pos < data.Length; pos++)
                data[pos] ^= polynomial[pos];
        }

        private static void ShiftLeft(bool[] remainder, bool[] srcData, int pos)
        {
            for (int copyPos = 0; copyPos < remainder.Length - 1; copyPos++)
                remainder[copyPos] = remainder[copyPos + 1];

            remainder[remainder.Length - 1] = pos < srcData.Length && srcData[pos];
        }

        public static bool[] Calc(bool[] data, bool[] polynomial)
        {
            return Calc(data, polynomial, null);
        }

        public static bool[] Calc(bool[] data, bool[] polynom, bool[] result)
        {
            return Calc(data, 0, data.Length, polynom, result);
        }

        public static bool[] Calc(bool[] data, int startPos, int length, bool[] polynomial)
        {
            return Calc(data, startPos, length, polynomial, null);
        }

        public static bool[] Calc(bool[] data, int startPos, int length, bool[] polynomial, bool[] result)
        {
            int polyLen = polynomial.Length;

            if (result == null)
                result = new bool[polyLen - 1];

            bool[] remainder = new bool[polyLen];

            Array.Copy(data, startPos, remainder, 0, polyLen);
            for (int pos = startPos + polyLen; pos < length; pos++)
            {
                DivMod2(remainder, polynomial);
                ShiftLeft(remainder, data, pos);
            }
            DivMod2(remainder, polynomial);

            Array.Copy(remainder, 1, result, 0, result.Length);

            return result;
        }

        public static bool Matches(bool[] data)
        {
            foreach (bool b in data)
                if (!b)
                    return false;

            return true;
        }

        public static bool Matches(bool[] data, bool[] polynomial)
        {
            for (int pos = 1; pos < polynomial.Length; pos++)
                if (data[pos - 1] != polynomial[pos])
                    return false;

            return true;
        }
    }
}