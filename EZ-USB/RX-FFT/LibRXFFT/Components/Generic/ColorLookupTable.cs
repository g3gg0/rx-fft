using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace LibRXFFT.Components.Generic
{
    public class ColorLookupTable
    {
        public Color ColorBg = Color.Black;
        public Color ColorFg = Color.White;

        public ColorLookupTable()
        { 
        }

        public ColorLookupTable(Color colorFg) : this(colorFg, Color.Black) { }

        public ColorLookupTable(Color colorFg, Color colorBg)
        {
            ColorFg = colorFg;
            ColorBg = colorBg;
        }

        public virtual uint Lookup(double relPos)
        {
            if (double.IsNaN(relPos))
            {
                return 0;
            }

            uint colorCode = 0;
            int rDelta = ColorFg.R - ColorBg.R;
            int gDelta = ColorFg.G - ColorBg.G;
            int bDelta = ColorFg.B - ColorBg.B;

            colorCode <<= 8;
            colorCode |= (uint)(ColorBg.R + rDelta * relPos);
            colorCode <<= 8;
            colorCode |= (uint)(ColorBg.G + gDelta * relPos);
            colorCode <<= 8;
            colorCode |= (uint)(ColorBg.B + bDelta * relPos);

            return colorCode;
        }
    }
}
