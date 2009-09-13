using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibRXFFT.Libraries.Misc;

namespace LibRXFFT.Components.GDI
{
    public class FrequencySelector : TextBox
    {
        public long LowerLimit = 0;
        public long UpperLimit = 8000000000;

        public FrequencySelector()
            : base()
        {
            this.MouseWheel += new System.Windows.Forms.MouseEventHandler(FrequencySelector_MouseWheel);
            this.KeyPress += new KeyPressEventHandler(FrequencySelector_KeyPress);
        }

        void FrequencySelector_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 0x0D)
            {
                int oldSelStart = SelectionStart;
                Frequency = Frequency;
                SelectionStart = oldSelStart;
            }
        }

        public long FrequencyUnitFactor
        {
            get
            {
                long factor = 1;

                switch (FrequencyUnitString)
                {
                    case "Hz":
                        return 1;
                    case "kHz":
                        return 1000;
                    case "MHz":
                        return 1000000;
                    case "GHz":
                        return 1000000000;
                    default:
                        return 1;
                }
            }
        }


        public string FrequencyUnitString
        {
            get
            {
                string[] parts = Text.Split(' ');

                if (parts.Length != 2)
                {
                    return "Hz";
                }

                return parts[1];
            }
        }

        public long Frequency
        {
            get
            {
                long factor = FrequencyUnitFactor;
                string[] parts = Text.Split(' ');

                if (parts.Length < 1)
                {
                    return 0;
                }


                string freqString = parts[0].Replace(".", "");
                decimal freqValue = 0;

                if (!decimal.TryParse(freqString, out freqValue))
                {
                    return 0;
                }

                long frequency = (long)(freqValue * factor);
                return frequency;
            }

            set
            {
                if (Text != "")
                    Text = FrequencyFormatter.FreqToStringAccurate(value);

            }
        }

        void FrequencySelector_MouseWheel(object sender, MouseEventArgs e)
        {
            string origText = Text;
            int oldSelectionStart = SelectionStart;
            long origFreq = Frequency;
            long newFreq = origFreq;
            int freqDecades = origFreq.ToString().Length;
            int decade = freqDecades;

            /* check at which decade the cursor is positioned */
            for (int pos = 0; pos < origText.Length; pos++)
            {
                if (pos < oldSelectionStart && char.IsDigit(origText[pos]))
                    decade--;
            }

            /* add/subtract the mouse wheel actions */
            newFreq += Math.Sign(e.Delta) * ((long)Math.Pow(10, (double)decade));

            newFreq = Math.Min(UpperLimit, newFreq);
            newFreq = Math.Max(LowerLimit, newFreq);

            Frequency = newFreq;

            /* reposition the cursor */
            int newSelStart = oldSelectionStart + (Text.Length - origText.Length);
            if (newSelStart >= 0)
                SelectionStart = newSelStart;
        }
    }
}
