using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace LibRXFFT.Components.Generic
{
    public class MultiColorMap : ColorLookupTable
    {
        private uint[] ColorMap;
        private int Resolution;
        private Color[] Colors;

        public MultiColorMap(int resolution, params Color[] colors)
        {
            Resolution = resolution;
            Colors = colors;

            FillColors();
        }

        private void FillColors()
        {
            int offset = 0;
            int stepsPerColor = Resolution / (Colors.Length - 1);

            double r;
            double g;
            double b;

            ColorMap = new uint[Resolution];

            for (int color = 0; color < Colors.Length - 1; color++)
            {
                Color color1 = Colors[color];
                Color color2 = Colors[color + 1];

                for (int pos = 0; pos < stepsPerColor; pos++)
                {
                    double fract = (double)pos / stepsPerColor;

                    r = color1.R * (1 - fract) + color2.R * fract;
                    g = color1.G * (1 - fract) + color2.G * fract;
                    b = color1.B * (1 - fract) + color2.B * fract;

                    ColorMap[offset + pos] = (((uint)r) << 16) | (((uint)g) << 8) | (((uint)b) << 0);
                }
                offset += stepsPerColor;
            }

            /*  fill remaining slots with final */
            for (int pos = offset; pos < Resolution; pos++)
            {
                ColorMap[pos] = (uint)(Colors[Colors.Length - 1].ToArgb() & 0xFFFFFF);
            }
        }

        public override uint Lookup(double relPos)
        {
            if (double.IsNaN(relPos))
            {
                return 0;
            }
            return ColorMap[(int)Math.Min(Resolution - 1, relPos * Resolution)];
        }
    }
}
