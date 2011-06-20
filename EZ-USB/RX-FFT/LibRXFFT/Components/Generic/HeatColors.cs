using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace LibRXFFT.Components.Generic
{
    public class HeatColors : ColorLookupTable
    {
        private uint[] ColorMap;
        private int Resolution;

        public HeatColors(int resolution)
        {
            Resolution = resolution;

            FillColors();
        }

        private void FillColors()
        {
            int offset = 0;
            int stepsPerColor = Resolution / 6;

            ColorMap = new uint[Resolution];

            /* raise B */
            for (int pos = 0; pos < stepsPerColor; pos++)
            {
                double fract = (double)pos / stepsPerColor;
                ColorMap[offset + pos] = 0x000000 | (uint)(255.0 * fract);
            }
            offset += stepsPerColor;

            /* raise G */
            for (int pos = 0; pos < stepsPerColor; pos++)
            {
                double fract = (double)pos / stepsPerColor;
                ColorMap[offset + pos] = 0x0000FF | ((uint)(255.0 * fract) << 8);
            }
            offset += stepsPerColor;

            /* reduce B */
            for (int pos = 0; pos < stepsPerColor; pos++)
            {
                double fract = (double)pos / stepsPerColor;
                ColorMap[offset + pos] = 0x00FF00 | (uint)(255.0 * (1 - fract));
            }
            offset += stepsPerColor;

            /* raise R */
            for (int pos = 0; pos < stepsPerColor; pos++)
            {
                double fract = (double)pos / stepsPerColor;
                ColorMap[offset + pos] = 0x00FF00 | ((uint)(255.0 * fract) << 8);
            }
            offset += stepsPerColor;

            /* reduce G */
            for (int pos = 0; pos < stepsPerColor; pos++)
            {
                double fract = (double)pos / stepsPerColor;
                ColorMap[offset + pos] = 0xFF0000 | ((uint)(255.0 * (1 - fract)) << 8);
            }
            offset += stepsPerColor;

            /* raise RB */
            for (int pos = 0; pos < stepsPerColor; pos++)
            {
                double fract = (double)pos / stepsPerColor;
                ColorMap[offset + pos] = 0xFF0000 | ((uint)(255.0 * fract) << 8) | ((uint)(255.0 * fract) << 16);
            }
            offset += stepsPerColor;

            /*  fill remaining slots with white */
            for (int pos = offset; pos < Resolution; pos++)
            {
                ColorMap[pos] = 0xFFFFFF;
            }

        }

        public override uint Lookup(double relPos)
        {
            if (double.IsNaN(relPos))
            {
                return 0;
            }
            return ColorMap[(int)Math.Min(Resolution-1, relPos * Resolution)];
        }
    }
}
