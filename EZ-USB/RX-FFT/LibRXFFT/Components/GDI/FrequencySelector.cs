using System;
using System.Drawing;
using System.Windows.Forms;
using LibRXFFT.Libraries.Misc;
using System.Globalization;

namespace LibRXFFT.Components.GDI
{
    public class FrequencySelector : TextBox
    {
        public long LowerLimit = 0;
        public long UpperLimit = 8000000000;

        public bool FrequencyValid = false;
        public long CurrentFrequency = 0;

        public event EventHandler FrequencyChanged;
        public event EventHandler EnterPresed;

        private char DecimalSeparatorChar;
        private string FixedLengthFormat = "";

        public FrequencySelector()
            : base()
        {
            this.MouseWheel += new MouseEventHandler(FrequencySelector_MouseWheel);
            this.KeyPress += new KeyPressEventHandler(FrequencySelector_KeyPress);

            FixedLengthString = false;
            FixedLengthDecades = 10;
            DecimalSeparatorChar = DecimalSeparator();
        }

        private string BuildFixedLengthFormat(int decades)
        {
            int groups = decades / 3;
            int remain = decades % 3;
            string format = "";

            /* "{0:0,000,000,000} Hz" */

            for (int pos = 0; pos < groups; pos++)
            {
                if (pos == 0)
                {
                    format = "000" + format;
                }
                else
                {
                    format = "000," + format;
                }
            }
            for (int pos = 0; pos < remain; pos++)
            {
                if (pos == 0 && groups > 0)
                {
                    format = "0," + format;
                }
                else
                {
                    format = "0" + format;
                }
            }

            return "{0:" + format + "} Hz";
        }

        void FrequencySelector_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (ReadOnly)
            {
                e.Handled = false;
                return;
            }

            ParseFrequency();

            if (e.KeyChar == 0x0D)
            {                
                /* reformat frequency */
                Frequency = CurrentFrequency;

                if (FrequencyValid)
                {
                    ForeColor = Color.Black;
                }
                else
                {
                    ForeColor = Color.Red;
                }

                if (FrequencyChanged != null)
                    FrequencyChanged(this, null);

                if (EnterPresed != null)
                    EnterPresed(this, null);

                e.Handled = true;
            }
        }

        public static char DecimalSeparator()
        {
            return CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator.ToCharArray()[0];
        }

        void FrequencySelector_MouseWheel(object sender, MouseEventArgs e)
        {
            if (ReadOnly)
            {
                return;
            }

            string origText = Text;
            int oldSelectionStart = SelectionStart;
            long origFreq = Frequency;
            long newFreq = origFreq;
            int freqDecades = 0;

            /* get the number of decades for the current frequency */
            for (int pos = 0; pos < origText.Length; pos++)
            {
                if (char.IsDigit(origText[pos]))
                {
                    freqDecades++;
                }

                if (origText[pos] == DecimalSeparatorChar)
                {
                    break;
                }
            }
            freqDecades += FrequencyUnitDecades;

            /* check at which decade the cursor is positioned */
            int decade = freqDecades;
            for (int pos = 0; pos < origText.Length; pos++)
            {
                if (pos < oldSelectionStart && char.IsDigit(origText[pos]))
                {
                    decade--;
                }
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

            if (FrequencyValid)
            {
                ForeColor = Color.Black;
            }
            else
            {
                ForeColor = Color.Red;
            }

            if (FrequencyChanged != null)
                FrequencyChanged(this, null);
        }

        public void ParseFrequency()
        {
            long factor = FrequencyUnitFactor;
            string trimmedText = Text.Trim();
            string[] parts = trimmedText.Split(' ');
            string freqString;

            /* found two parts, so the first should be the number to parse */
            if (parts.Length == 2)
            {
                freqString = parts[0];
            }
            else
            {
                /* first strategy did not work, maybe unit is added without space like "100,2MHz" */

                /* skip the digits and commas */
                int pos = 0;

                while (pos < trimmedText.Length && (char.IsDigit(trimmedText[pos]) || trimmedText[pos] == ',' || trimmedText[pos] == '.'))
                {
                    pos++;
                }

                /* if not reached the end, the rest string is the unit */
                if (pos < Text.Length)
                {
                    freqString = trimmedText.Substring(0, pos);
                }
                else
                {
                    /* try our best with the string we have */
                    freqString = trimmedText;
                }
            }

            /* remove dots. "1.208,5 MHz" is possible that way */
            freqString = freqString.Replace(".", "");
            decimal freqValue = 0;

            if (!decimal.TryParse(freqString, out freqValue))
            {
                FrequencyValid = false;
                return;
            }

            FrequencyValid = true;
            CurrentFrequency = (long)(freqValue * factor); 
        }

        public long FrequencyUnitFactor
        {
            get
            {
                return (long)Math.Pow(10, FrequencyUnitDecades);
            }
        }

        public int FrequencyUnitDecades
        {
            get
            {
                long factor = 1;

                switch (FrequencyUnitString.ToLower())
                {
                    case "hz":
                        return 0;
                    case "khz":
                        return 3;
                    case "mhz":
                        return 6;
                    case "ghz":
                        return 9;
                    default:
                        return 0;
                }
            }
        }

        public string FrequencyUnitString
        {
            get
            {
                string[] parts = Text.Split(' ');

                /* the string was split into two strings. so the second part must be the unit */
                if (parts.Length == 2)
                {
                    return parts[1];
                }

                /* skip the digits and commas */
                int pos = 0;

                while (pos < Text.Length && (char.IsDigit(Text[pos]) || Text[pos] == ',' || Text[pos] == '.'))
                {
                    pos++;
                }

                /* if not reached the end, return the rest string */
                if(pos < Text.Length)
                {
                    return Text.Substring(pos);
                }

                return "Hz";
            }
        }

        private int _FixedLengthDecades;
        public int FixedLengthDecades
        {
            get
            {
                return _FixedLengthDecades;
            }
            set
            {
                _FixedLengthDecades = value;
                FixedLengthFormat = BuildFixedLengthFormat(FixedLengthDecades);
            }
        }

        public bool FixedLengthString
        {
            get;
            set;
        }

        public long Frequency
        {
            get
            {
                /* make sure its already parsed (may be missed when ctrl-v was pressed) */
                ParseFrequency();

                return CurrentFrequency;
            }
            set
            {
                if (FixedLengthString)
                {
                    Text = String.Format(FixedLengthFormat, value);
                }
                else
                {
                    Text = FrequencyFormatter.FreqToStringAccurate(value);
                }
                ParseFrequency();
            }
        }
    }
}
