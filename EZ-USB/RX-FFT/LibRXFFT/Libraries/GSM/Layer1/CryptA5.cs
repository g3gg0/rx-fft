/*
 * A pedagogical implementation of A5/1.
 *
 * Copyright (C) 1998-1999: Marc Briceno, Ian Goldberg, and David Wagner
 *
 * The source code below is optimized for instructional value and clarity.
 * Performance will be terrible, but that's not the point.
 * The algorithm is written in the C programming language to avoid ambiguities
 * inherent to the English language. Complain to the 9th Circuit of Appeals
 * if you have a problem with that.
 *
 * This software may be export-controlled by US law.
 *
 * This software is free for commercial and non-commercial use as long as
 * the following conditions are aheared to.
 * Copyright remains the authors' and as such any Copyright notices in
 * the code are not to be removed.
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 *
 * 1. Redistributions of source code must retain the copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED ``AS IS'' AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED.  IN NO EVENT SHALL THE AUTHORS OR CONTRIBUTORS BE LIABLE
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
 * OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
 * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
 * OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
 * SUCH DAMAGE.
 * 
 * The license and distribution terms for any publicly available version or
 * derivative of this code cannot be changed.  i.e. this code cannot simply be
 * copied and put under another distribution license
 * [including the GNU Public License.]
 *
 * Background: The Global System for Mobile communications is the most widely
 * deployed cellular telephony system in the world. GSM makes use of
 * four core cryptographic algorithms, neither of which has been published by
 * the GSM MOU. This failure to subject the algorithms to public review is all  
 * the more puzzling given that over 100 million GSM
 * subscribers are expected to rely on the claimed security of the system.
 *
 * The four core GSM algorithms are:
 * A3		authentication algorithm
 * A5/1		"strong" over-the-air voice-privacy algorithm
 * A5/2		"weak" over-the-air voice-privacy algorithm
 * A8		voice-privacy key generation algorithm
 *
 * In April of 1998, our group showed that COMP128, the algorithm used by the
 * overwhelming majority of GSM providers for both A3 and A8
 * functionality was fatally flawed and allowed for cloning of GSM mobile
 * phones.
 * Furthermore, we demonstrated that all A8 implementations we could locate,
 * including the few that did not use COMP128 for key generation, had been
 * deliberately weakened by reducing the keyspace from 64 bits to 54 bits.
 * The remaining 10 bits are simply set to zero!
 *
 * See http://www.scard.org/gsm for additional information.
 *
 * The question so far unanswered is if A5/1, the "stronger" of the two
 * widely deployed voice-privacy algorithm is at least as strong as the
 * key. Meaning: "Does A5/1 have a work factor of at least 54 bits"?
 * Absent a publicly available A5/1 reference implementation, this question
 * could not be answered. We hope that our reference implementation below,
 * which has been verified against official A5/1 test vectors, will provide
 * the cryptographic community with the base on which to construct the
 * answer to this important question.
 *
 * Initial indications about the strength of A5/1 are not encouraging.
 * A variant of A5, while not A5/1 itself, has been estimated to have a
 * work factor of well below 54 bits. See http://jya.com/crack-a5.htm for
 * background information and references.
 * 
 * With COMP128 broken and A5/1 published below, we will now turn our attention
 * to A5/2. The latter has been acknowledged by the GSM community to have
 * been specifically designed by intelligence agencies for lack of security.
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using bit = System.UInt32;
using RX_FFT.Components.GDI;

namespace LibRXFFT.Libraries.GSM.Layer1
{
    public class CryptA5
    {
        public byte[] Key = new byte[8];
        public uint Count = 0;
        public bool[] DownlinkKey = new bool[114];
        public bool[] UplinkKey = new bool[114];

        /* The three shift registers. */
        private uint R1 = 0;
        private uint R2 = 0;
        private uint R3 = 0;

        /* Masks for the three shift registers */
        const uint R1MASK = 0x07FFFF; /* 19 bits, numbered 0..18 */
        const uint R2MASK = 0x3FFFFF; /* 22 bits, numbered 0..21 */
        const uint R3MASK = 0x7FFFFF; /* 23 bits, numbered 0..22 */

        /* Middle bit of each of the three shift registers, for clock control */
        const uint R1MID = 0x000100; /* bit 8 */
        const uint R2MID = 0x000400; /* bit 10 */
        const uint R3MID = 0x000400; /* bit 10 */

        /* Feedback taps, for clocking the shift registers.
         * These correspond to the primitive polynomials
         * x^19 + x^5 + x^2 + x + 1, x^22 + x + 1,
         * and x^23 + x^15 + x^2 + x + 1. */
        const uint R1TAPS = 0x072000; /* bits 18,17,16,13 */
        const uint R2TAPS = 0x300000; /* bits 21,20 */
        const uint R3TAPS = 0x700080; /* bits 22,21,20,7 */

        /* Output taps, for output generation */
        const uint R1OUT = 0x040000; /* bit 18 (the high bit) */
        const uint R2OUT = 0x200000; /* bit 21 (the high bit) */
        const uint R3OUT = 0x400000; /* bit 22 (the high bit) */

        public CryptA5()
        {
        }

        public CryptA5(byte[] key)
        {
            Key = key;
        }

        /* Calculate the parity of a 32-bit word, i.e. the sum of its bits modulo 2 */
        bit parity(uint x)
        {
            x ^= x >> 16;
            x ^= x >> 8;
            x ^= x >> 4;
            x ^= x >> 2;
            x ^= x >> 1;

            return x & 1;
        }

        /* Clock one shift register */
        void clockone(ref uint reg, uint mask, uint taps)
        {
            uint t = reg & taps;

            reg = (reg << 1) & mask;
            reg |= parity(t);
        }

        /* Look at the middle bits of R1,R2,R3, take a vote, and
         * return the majority value of those 3 bits. */
        bit majority()
        {
            uint sum = parity(R1 & R1MID) + parity(R2 & R2MID) + parity(R3 & R3MID);

            return (bit)((sum >= 2) ? 1 : 0);
        }

        /* Clock two or three of R1,R2,R3, with clock control
         * according to their middle bits.
         * Specifically, we clock Ri whenever Ri's middle bit
         * agrees with the majority value of the three middle bits.*/
        void clock()
        {
            bit maj = majority();

            if (parity(R1 & R1MID) == maj)
                clockone(ref R1, R1MASK, R1TAPS);
            if (parity(R2 & R2MID) == maj)
                clockone(ref R2, R2MASK, R2TAPS);
            if (parity(R3 & R3MID) == maj)
                clockone(ref R3, R3MASK, R3TAPS);
        }

        /* Clock all three of R1,R2,R3, ignoring their middle bits.
         * This is only used for key setup. */
        void clockallthree()
        {
            clockone(ref R1, R1MASK, R1TAPS);
            clockone(ref R2, R2MASK, R2TAPS);
            clockone(ref R3, R3MASK, R3TAPS);
        }

        /* Generate an output bit from the current state.
         * You grab a bit from each register via the output generation taps;
         * then you XOR the resulting three bits. */
        bit getbit()
        {
            return parity(R1 & R1OUT) ^ parity(R2 & R2OUT) ^ parity(R3 & R3OUT);
        }

        /* Generate output.  We generate 228 bits of
         * keystream output.  The first 114 bits is for
         * the A->B frame; the next 114 bits is for the
         * B->A frame. */
        public void RunAlgo()
        {
            int i;

            KeySetup();

            /* Generate 114 bits of keystream for the A->B direction. */
            for (i = 0; i < 114; i++)
            {
                clock();
                DownlinkKey[i] = (getbit() != 0);
            }

            /* Generate 114 bits of keystream for the B->A direction. */
            for (i = 0; i < 114; i++)
            {
                clock();
                UplinkKey[i] = (getbit() != 0);
            }
        }

        /* Do the A5/1 key setup.  This routine accepts a 64-bit key and a 22-bit frame number. */
        void KeySetup()
        {
            KeySetup(Key, Count);
        }

        void KeySetup(byte[] key, uint frame)
        {
            /* Zero out the shift registers. */
            R1 = R2 = R3 = 0;

            /* Load the key into the shift registers,
             * LSB of first byte of key array first,
             * clocking each register once for every
             * key bit loaded.  (The usual clock
             * control rule is temporarily disabled.) */
            for (int i = 0; i < 64; i++)
            {
                int bitNum = (i % 8);
                int byteNum = 7 - (i / 8);

                clockallthree(); /* always clock */
                bit keybit = (uint)(key[byteNum] >> bitNum) & 1; /* The i-th bit of the key */
                R1 ^= keybit; 
                R2 ^= keybit; 
                R3 ^= keybit;
            }

            /* Load the frame number into the shift
             * registers, LSB first,
             * clocking each register once for every
             * key bit loaded.  (The usual clock
             * control rule is still disabled.) */
            for (int i = 0; i < 22; i++)
            {
                clockallthree(); /* always clock */
                bit framebit = (frame >> i) & 1; /* The i-th bit of the frame # */
                R1 ^= framebit; 
                R2 ^= framebit; 
                R3 ^= framebit;
            }

            /* Run the shift registers for 100 clocks
             * to mix the keying material and frame number
             * together with output generation disabled,
             * so that there is sufficient avalanche.
             * We re-enable the majority-based clock control
             * rule from now on. */
            for (int i = 0; i < 100; i++)
            {
                clock();
            }

            /* Now the key is properly set up. */
        }

        /* Test the code by comparing it against a known-good test vector. */
        public static void SelfCheck()
        {
            uint testFrame = 0x134;
            byte[] testKey = new byte[] { 0xEF, 0xCD, 0xAB, 0x89, 0x67, 0x45, 0x23, 0x12 };
            bool[] goodAtoB = ByteUtil.BitsFromBytes(new byte[] { 0x53, 0x4E, 0xAA, 0x58, 0x2F, 0xE8, 0x15, 0x1A, 0xB6, 0xE1, 0x85, 0x5A, 0x72, 0x8C, 0x00 });
            bool[] goodBtoA = ByteUtil.BitsFromBytes(new byte[] { 0x24, 0xFD, 0x35, 0xA3, 0x5D, 0x5F, 0xB6, 0x52, 0x6D, 0x32, 0xF9, 0x06, 0xDF, 0x1A, 0xC0 });
            int i = 0;
            bool failed = false;

            CryptA5 a5 = new CryptA5();
            a5.Key = testKey;
            a5.Count = testFrame;
            a5.RunAlgo();

            /* Compare against the test vector. */
            for (i = 0; i < 114; i++)
                if (a5.DownlinkKey[i] != goodAtoB[i])
                    failed = true;
            for (i = 0; i < 114; i++)
                if (a5.UplinkKey[i] != goodBtoA[i])
                    failed = true;

            if (failed)
            {
                /* Problems!  The test vectors didn't compare*/
                StringBuilder builder = new StringBuilder();

                /* Print some debugging output. */
                builder.Append("key: 0x");
                for (i = 0; i < 8; i++)
                    builder.AppendFormat("{0:X02}", testKey[i]);
                builder.Append("\r\n");
                builder.AppendFormat("frame number: 0x{0:X6}\r\n", (uint)testFrame);
                builder.Append("known good output:\r\n");
                builder.Append(" A->B: ");
                for (i = 0; i < 114; i++)
                    builder.Append(goodAtoB[i] ? 1 : 0);
                builder.Append("  B->A: ");
                for (i = 0; i < 114; i++)
                    builder.Append(goodBtoA[i] ? 1 : 0);
                builder.Append("\r\n");
                builder.Append("observed output:\r\n");
                builder.Append(" A->B: ");
                for (i = 0; i < 114; i++)
                    builder.Append(a5.DownlinkKey[i] ? 1 : 0);
                builder.Append("  B->A: ");
                for (i = 0; i < 114; i++)
                    builder.Append(a5.UplinkKey[i] ? 1 : 0);
                builder.Append("\r\n");

                builder.Append("\r\nI don't know why this broke; contact the authors.\r\n");
                Log.AddMessage(builder.ToString());
            }
        }

        public void CryptDownlink(bool[] burstBufferE, uint frame)
        {
            Count = frame;
            RunAlgo();

            /* GSM 03.20 Ch. 3.1.2 */
            for (int pos = 0; pos < 114; pos++)
            {
                burstBufferE[pos] ^= DownlinkKey[pos];
            }
        }

        public void CryptUplink(bool[] burstBufferE, uint frame)
        {
            Count = frame;
            RunAlgo();

            /* GSM 03.20 Ch. 3.1.2 */
            for (int pos = 0; pos < 114; pos++)
            {
                burstBufferE[pos] ^= UplinkKey[pos];
            }
        }
    }
}
