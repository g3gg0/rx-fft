#warning "This file contains GPLv2 code (DecodeViterbi). This code must be rewritten from scratch."

namespace LibRXFFT.Libraries.GSM.Layer1.ChannelCoding
{
    public class ConvolutionalCoder
    {
        private static bool Bit(bool[] srcData, int pos)
        {
            if (pos < 0)
                return false;

            return srcData[pos];
        }

        public static int Decode(bool[] srcData, ref bool[] dstData)
        {
            bool failed = false;
            int errors = 0;

            if (dstData == null)
                dstData = new bool[srcData.Length / 2];

            for (int bitPos = 0; bitPos < srcData.Length / 2; bitPos++)
            {
                bool bit1 = Bit(srcData, 2 * bitPos) ^ Bit(dstData, bitPos - 3) ^ Bit(dstData, bitPos - 4);
                bool bit2 = Bit(srcData, 2 * bitPos + 1) ^ Bit(dstData, bitPos - 1) ^ Bit(dstData, bitPos - 3) ^ Bit(dstData, bitPos - 4);

                /* decoding failed? use viterbi to decode */
                if (bit1 != bit2)
                    failed = true;

                dstData[bitPos] = bit1;
            }

            if (failed && (srcData.Length == 456))
            {
                errors = DecodeViterbi(srcData, ref dstData);

                failed = (errors > 10);
            }

            if (failed)
            {
                return int.MaxValue;
            }

            return errors;
        }



        /*
         * Convolutional encoding and Viterbi decoding for the GSM SACCH channel.
         * 
         * from airprobe, /gsm-tvoid/src/lib/cch.c
         * 
         * License: GPLv2
         * 
         */

        /*
         * Copyright 2005 Free Software Foundation, Inc.
         * 
         * This file is part of GNU Radio
         * 
         * GNU Radio is free software; you can redistribute it and/or modify
         * it under the terms of the GNU General Public License as published by
         * the Free Software Foundation; either version 2, or (at your option)
         * any later version.
         * 
         * GNU Radio is distributed in the hope that it will be useful,
         * but WITHOUT ANY WARRANTY; without even the implied warranty of
         * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
         * GNU General Public License for more details.
         * 
         * You should have received a copy of the GNU General Public License
         * along with GNU Radio; see the file COPYING.  If not, write to
         * the Free Software Foundation, Inc., 59 Temple Place - Suite 330,
         * Boston, MA 02111-1307, USA.
         */


        /*
         * Convolutional encoding:
         *
         *	G_0 = 1 + x^3 + x^4
         *	G_1 = 1 + x + x^3 + x^4
         *
         * i.e.,
         *
         *	c_{2k} = u_k + u_{k - 3} + u_{k - 4}
         *	c_{2k + 1} = u_k + u_{k - 1} + u_{k - 3} + u_{k - 4}
         */

        private static int K = 5;
        private static uint CONV_INPUT_SIZE = 228;
        private static uint MAX_ERROR = (2 * CONV_INPUT_SIZE + 1);


        /*
         * Given the current state and input bit, what are the output bits?
         *
         * 	encode[current_state,input_bit]
         */
        static uint[,] encode = {
	        {0, 3}, {3, 0}, {3, 0}, {0, 3},
	        {0, 3}, {3, 0}, {3, 0}, {0, 3},
	        {1, 2}, {2, 1}, {2, 1}, {1, 2},
	        {1, 2}, {2, 1}, {2, 1}, {1, 2}
        };


        /*
         * Given the current state and input bit, what is the next state?
         * 
         * 	next_state[current_state,input_bit]
         */
        static uint[,] next_state = {
	        {0, 8}, {0, 8}, {1, 9}, {1, 9},
	        {2, 10}, {2, 10}, {3, 11}, {3, 11},
	        {4, 12}, {4, 12}, {5, 13}, {5, 13},
	        {6, 14}, {6, 14}, {7, 15}, {7, 15}
        };


        /*
         * Given the previous state and the current state, what input bit caused
         * the transition?  If it is impossible to transition between the two
         * states, the value is 2.
         *
         * 	prev_next_state[previous_state,current_state]
         */
        static uint[,] prev_next_state = {
                { 0,  2,  2,  2,  2,  2,  2,  2,  1,  2,  2,  2,  2,  2,  2,  2},
                { 0,  2,  2,  2,  2,  2,  2,  2,  1,  2,  2,  2,  2,  2,  2,  2},
                { 2,  0,  2,  2,  2,  2,  2,  2,  2,  1,  2,  2,  2,  2,  2,  2},
                { 2,  0,  2,  2,  2,  2,  2,  2,  2,  1,  2,  2,  2,  2,  2,  2},
                { 2,  2,  0,  2,  2,  2,  2,  2,  2,  2,  1,  2,  2,  2,  2,  2},
                { 2,  2,  0,  2,  2,  2,  2,  2,  2,  2,  1,  2,  2,  2,  2,  2},
                { 2,  2,  2,  0,  2,  2,  2,  2,  2,  2,  2,  1,  2,  2,  2,  2},
                { 2,  2,  2,  0,  2,  2,  2,  2,  2,  2,  2,  1,  2,  2,  2,  2},
                { 2,  2,  2,  2,  0,  2,  2,  2,  2,  2,  2,  2,  1,  2,  2,  2},
                { 2,  2,  2,  2,  0,  2,  2,  2,  2,  2,  2,  2,  1,  2,  2,  2},
                { 2,  2,  2,  2,  2,  0,  2,  2,  2,  2,  2,  2,  2,  1,  2,  2},
                { 2,  2,  2,  2,  2,  0,  2,  2,  2,  2,  2,  2,  2,  1,  2,  2},
                { 2,  2,  2,  2,  2,  2,  0,  2,  2,  2,  2,  2,  2,  2,  1,  2},
                { 2,  2,  2,  2,  2,  2,  0,  2,  2,  2,  2,  2,  2,  2,  1,  2},
                { 2,  2,  2,  2,  2,  2,  2,  0,  2,  2,  2,  2,  2,  2,  2,  1},
                { 2,  2,  2,  2,  2,  2,  2,  0,  2,  2,  2,  2,  2,  2,  2,  1}
        };

        static uint[] HammingDistance2 = { 0, 1, 1, 2 };


        public static int DecodeViterbi(bool[] srcData, ref bool[] dstData)
        {
            if (srcData.Length != 2 * CONV_INPUT_SIZE)
            {
                return -1;
            }

            if (dstData == null)
                dstData = new bool[srcData.Length / 2];

            int i, t;
            uint rdata, state, nstate, b, o, distance, accumulated_error, min_state, min_error, cur_state;

            int steps = 1 << (K - 1);
            uint[] ae = new uint[steps];
            uint[] nae = new uint[steps]; // next accumulated error
            uint[,] state_history = new uint[steps, CONV_INPUT_SIZE + 1];

            // initialize accumulated error, assume starting state is 0
            for (i = 0; i < steps; i++)
            {
                ae[i] = MAX_ERROR;
                nae[i] = MAX_ERROR;
            }
            ae[0] = 0;

            // build trellis
            for (t = 0; t < CONV_INPUT_SIZE; t++)
            {
                // get received data symbol
                rdata = 0;
                if (srcData[2 * t])
                    rdata |= 2;
                if (srcData[2 * t + 1])
                    rdata |= 1;

                // for each state
                for (state = 0; state < steps; state++)
                {
                    // make sure this state is possible
                    if (ae[state] >= MAX_ERROR)
                        continue;

                    // find all states we lead to
                    for (b = 0; b < 2; b++)
                    {
                        // get next state given input bit b
                        nstate = next_state[state, b];

                        // find output for this transition
                        o = encode[state, b];

                        // calculate distance from received data
                        distance = HammingDistance2[rdata ^ o];

                        // choose surviving path
                        accumulated_error = ae[state] + distance;
                        if (accumulated_error < nae[nstate])
                        {
                            // save error for surviving state
                            nae[nstate] = accumulated_error;

                            // update state history
                            state_history[nstate, t + 1] = state;
                        }
                    }
                }

                // get accumulated error ready for next time slice
                for (i = 0; i < steps; i++)
                {
                    ae[i] = nae[i];
                    nae[i] = MAX_ERROR;
                }
            }

            // the final state is the state with the fewest errors
            min_state = uint.MaxValue;
            min_error = MAX_ERROR;
            for (i = 0; i < steps; i++)
            {
                if (ae[i] < min_error)
                {
                    min_state = (uint)i;
                    min_error = ae[i];
                }
            }

            // trace the path
            cur_state = min_state;
            for (t = (int)CONV_INPUT_SIZE; t >= 1; t--)
            {
                min_state = cur_state;
                cur_state = state_history[cur_state, t]; // get previous
                dstData[t - 1] = prev_next_state[cur_state, min_state] != 0;
            }

            // return the number of errors detected (hard-decision)
            return (int)min_error;
        }
    }
}
