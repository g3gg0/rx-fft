using System;

#warning "This file contains GPLv2 code (FireCode). This code must be rewritten from scratch."

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


namespace LibRXFFT.Libraries.GSM.Layer2
{
    public class FireCode
    {
        int crc_size;
        int data_size;
        int syn_start;
        bool[] syndrome_reg = new bool[40];

        public FireCode(int crcSize, int dataSize)
        {
            crc_size = crcSize;
            data_size = dataSize;
        }

        int REM(int x, int y)
        {
            return x % y;
        }

        public bool FC_check_crc(bool[] input_bits, bool[] control_data)
        {
            bool success_flag = false;
            int j = 0;
            int error_count = 0;
            int error_index = 0;
            int syn_index = 0;
            int i;

            syn_start = 0;
            // reset the syndrome register
            Array.Clear(syndrome_reg, 0, syndrome_reg.Length);

            // shift in the data bits
            for (i = 0; i < data_size; i++)
            {
                error_count = FC_syndrome_shift(input_bits[i]);
                control_data[i] = input_bits[i];
            }

            // shift in the crc bits
            for (i = 0; i < crc_size; i++)
            {
                error_count = FC_syndrome_shift(!input_bits[i + data_size]);
            }

            // Find position of error burst
            if (error_count == 0)
            {
                error_index = 0;
            }
            else
            {
                error_index = 1;
                error_count = FC_syndrome_shift(false);
                error_index += 1;
                while (error_index < (data_size + crc_size))
                {
                    error_count = FC_syndrome_shift(false);
                    error_index += 1;
                    if (error_count == 0) 
                        break;
                }
            }

            // Test for correctable errors
            //printf("error_index %d\n",error_index);
            if (error_index == 224)
                success_flag = false;
            else
            {
                // correct index depending on the position of the error
                if (error_index == 0)
                    syn_index = error_index;
                else
                    syn_index = error_index - 1;

                // error burst lies within data bits
                if (error_index < 184)
                {
                    //printf("error < bit 184,%d\n",error_index);
                    j = error_index;
                    while (j < (error_index + 12))
                    {
                        if (j < 184)
                        {
                            control_data[j] = control_data[j] ^
                               syndrome_reg[REM(syn_start + 39 - j + syn_index, 40)];
                        }
                        else 
                            break;
                        j = j + 1;
                    }
                }
                else if (error_index > 212)
                {
                    //printf("error > bit 212,%d\n",error_index);
                    j = 0;
                    while (j < (error_index - 212))
                    {
                        control_data[j] = control_data[j] ^
                              syndrome_reg[REM(syn_start + 39 - j - 224 + syn_index, 40)];
                        j = j + 1;
                    }
                }
                // for 183 < error_index < 213 error in parity alone so ignore
                success_flag = true;
            }
            return success_flag;
        }

        int FC_syndrome_shift(bool bit)
        {
            int error_count = 0;
            int i;

            if (syn_start == 0)
                syn_start = 39;
            else
                syn_start -= 1;

            bool[] temp_syndrome_reg = new bool[syndrome_reg.Length];

            Array.Copy(syndrome_reg, temp_syndrome_reg, temp_syndrome_reg.Length);

            temp_syndrome_reg[REM(syn_start + 3, 40)] = syndrome_reg[REM(syn_start + 3, 40)] ^ syndrome_reg[syn_start];
            temp_syndrome_reg[REM(syn_start + 17, 40)] = syndrome_reg[REM(syn_start + 17, 40)] ^ syndrome_reg[syn_start];
            temp_syndrome_reg[REM(syn_start + 23, 40)] = syndrome_reg[REM(syn_start + 23, 40)] ^ syndrome_reg[syn_start];
            temp_syndrome_reg[REM(syn_start + 26, 40)] = syndrome_reg[REM(syn_start + 26, 40)] ^ syndrome_reg[syn_start];

            temp_syndrome_reg[REM(syn_start + 4, 40)] = syndrome_reg[REM(syn_start + 4, 40)] ^ bit;
            temp_syndrome_reg[REM(syn_start + 6, 40)] = syndrome_reg[REM(syn_start + 6, 40)] ^ bit;
            temp_syndrome_reg[REM(syn_start + 10, 40)] = syndrome_reg[REM(syn_start + 10, 40)] ^ bit;
            temp_syndrome_reg[REM(syn_start + 16, 40)] = syndrome_reg[REM(syn_start + 16, 40)] ^ bit;
            temp_syndrome_reg[REM(syn_start + 27, 40)] = syndrome_reg[REM(syn_start + 27, 40)] ^ bit;
            temp_syndrome_reg[REM(syn_start + 29, 40)] = syndrome_reg[REM(syn_start + 29, 40)] ^ bit;
            temp_syndrome_reg[REM(syn_start + 33, 40)] = syndrome_reg[REM(syn_start + 33, 40)] ^ bit;
            temp_syndrome_reg[REM(syn_start + 39, 40)] = syndrome_reg[REM(syn_start + 39, 40)] ^ bit;

            temp_syndrome_reg[syn_start] = syndrome_reg[syn_start] ^ bit;

            Array.Copy(temp_syndrome_reg, syndrome_reg, syndrome_reg.Length);

            for (i = 0; i < 28; i++)
            {
                if (syndrome_reg[REM(syn_start + i, 40)])
                    error_count++;
            }
            return error_count;
        }
    }
}
