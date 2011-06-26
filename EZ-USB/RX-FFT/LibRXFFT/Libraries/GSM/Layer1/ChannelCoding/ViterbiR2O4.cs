using System.Numeric;

/* code origin OpenBTS / converted to C# */

namespace LibRXFFT.Libraries.GSM.Layer1.ChannelCoding
{
    public class ViterbiR204
    {

        private const uint mIRate = 2;
        private const uint mOrder = 4;

        /* the following can be declared as const if C# allows it */
        private static uint mIStates = (uint)0x01 << (int)mOrder;
        private static uint mSMask = mIStates - 1;
        private static uint mCMask = (mSMask << 1) | 0x01;
        private static uint mOMask = ((uint)0x01 << (int)mIRate) - 1;
        private static uint mNumCands = mIStates * 2;
        private static uint mDeferral = 6 * mOrder;			
        
        private static uint[] mCoeffs = new uint[mIRate];					
		private static uint[,] mStateTable = new uint[mIRate,2*mIStates];	
		private static uint[] mGeneratorTable = new uint[2*mIStates];		

        private struct cand
        {
            public static uint iState;
            public static uint oState;
            public static float cost;
        }

        private static cand[] mSurvivors = new cand[mIStates];
        private static cand[] mCandidates = new cand[2 * mIStates];

        private static uint applyPoly(ulong value, ulong poly, uint order)
        {
            ulong prod = value & poly;
            uint sum = (uint)prod;
            for ( uint i = 1; i < order; i++ )
                sum ^= (uint)(prod >> (int)i);

            return sum & 1;
        }

        private static void computeGeneratorTable()
        {
            for (uint index = 0; index < mIStates * 2; index++)
                mGeneratorTable[index] = (mStateTable[0,index] << 1) | mStateTable[1,index];

        }

        private static void computeStateTables(uint g)
        {
            for (uint state = 0; state < mIStates; state++)
            {
                uint input = state << 1;
                mStateTable[g,input] = applyPoly(input, mCoeffs[g], mOrder + 1);
                input |= 1;
                mStateTable[g,input] = applyPoly(input, mCoeffs[g], mOrder + 1);
            }
        }

        public static void init()
        {
            mCoeffs[0] = 0x19;
            mCoeffs[1] = 0x1b;

            computeStateTables(0);
            computeStateTables(1);
            computeGeneratorTable();
        }

        public static void encode(bool[] input, bool[] output )
        {
            uint[] history = new uint[input.Length];
            uint accum = 0;

            for ( int i = 0; i < input.Length; i++ )
            {
                accum = (accum<<1);
                accum |= (input[i] == true) ? (uint)1 : (uint)0;
                history[i] = accum;
            }

            for (int i = 0; i < input.Length; i++)
            {
                uint index = mCMask & history[i];
                for (int j = 0; j < mIRate; j++)
                {
                    output[i * mIRate + j] = (mStateTable[j, index] == 1) ? true : false;
                }

            }


        }
    }
}
