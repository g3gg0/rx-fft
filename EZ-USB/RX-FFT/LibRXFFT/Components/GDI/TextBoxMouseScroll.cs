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
    public class TextBoxMouseScroll : TextBox
    {
        public long LowerLimit { get; set; }
        public long UpperLimit { get; set; }

        public event EventHandler ValueChanged;

        public TextBoxMouseScroll()
            : base()
        {
            this.MouseWheel += new System.Windows.Forms.MouseEventHandler(TextBoxMouseScroll_MouseWheel);
            this.KeyPress += new KeyPressEventHandler(TextBoxMouseScroll_KeyPress);
        }

        void TextBoxMouseScroll_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (ReadOnly)
            {
                e.Handled = false;
                return;
            }

            if (e.KeyChar == 0x0D)
            {
                long newValue = Value;

                newValue = Math.Min(UpperLimit, newValue);
                newValue = Math.Max(LowerLimit, newValue);

                Value = newValue;
                if (ValueChanged != null)
                {
                    ValueChanged(this, null);
                }

                e.Handled = true;
            }
        }

        void TextBoxMouseScroll_MouseWheel(object sender, MouseEventArgs e)
        {
            if (ReadOnly)
            {
                return;
            }

            string origText = Text;
            int oldSelectionStart = SelectionStart;
            long origValue = Value;
            long newValue = origValue;
            int numDecades = origValue.ToString().Length;
            int decade = numDecades;

            /* check at which decade the cursor is positioned */
            for (int pos = 0; pos < origText.Length; pos++)
            {
                if (pos < oldSelectionStart && (char.IsDigit(origText[pos]) || origText[pos] == '-'))
                    decade--;
            }

            /* add/subtract the mouse wheel actions */
            newValue += Math.Sign(e.Delta) * ((long)Math.Pow(10, (double)decade));

            newValue = Math.Min(UpperLimit, newValue);
            newValue = Math.Max(LowerLimit, newValue);

            Value = newValue;

            /* reposition the cursor */
            int newSelStart = oldSelectionStart + (Text.Length - origText.Length);
            if (newSelStart >= 0)
                SelectionStart = newSelStart;

            if (ValueChanged != null)
            {
                ValueChanged(this, null);
            }
        }

        public long Value
        {
            get
            {
                long value = 0;
                if (long.TryParse(Text, out value))
                {
                    return value;
                }
                return 0;
            }
            set
            {
                Text = value.ToString();
            }
        }
    }
}
