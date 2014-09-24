using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;

namespace LibRXFFT.Components.GDI
{
    public class TextBoxMouseScrollDecimal : TextBoxMouseScroll<decimal>
    {
        protected override void Add(ref decimal newValue, int sign, int decade)
        {
            decimal value = 1;

            while (decade > 0)
            {
                value *= 10;
                decade--;
            }

            while (decade < 0)
            {
                value /= 10;
                decade++;
            }

            newValue += sign * value;
        }
    }

    public class TextBoxMouseScrollLong : TextBoxMouseScroll<long>
    {
        protected override void Add(ref long newValue, int sign, int decade)
        {
            newValue += (long)(sign * Math.Pow(10, decade));
        }
    }

    public class TextBoxMouseScroll<T> : TextBox
    {
        public T LowerLimit { get; set; }
        public T UpperLimit { get; set; }

        public event EventHandler ValueChanged;

        public TextBoxMouseScroll()
            : base()
        {
            this.MouseWheel += new MouseEventHandler(TextBoxMouseScroll_MouseWheel);
            this.KeyPress += new KeyPressEventHandler(TextBoxMouseScroll_KeyPress);
        }

        public bool TryParse(string s, Type type, out object result)
        {
            result = null;
            MethodInfo method = GetTryParseMethod(type);
            if (method == null)
            {
                throw new Exception("Invalid Type");
            }
            object[] parameters = new object[] { s, null };

            bool success = (bool)method.Invoke(null, parameters);
            if (success)
            {
                result = parameters[1];
            }

            return success;
        }

        public bool TryParse(string s, out T result)
        {
            result = default(T);
            object tempResult;
            bool success = TryParse(s, typeof(T), out tempResult);

            if (success)
            {
                result = (T)tempResult;
            }

            return success;
        }

        private static MethodInfo GetTryParseMethod(Type type)
        {
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static;
            Type[] parameterTypes = new Type[] { typeof(string), type.MakeByRefType() };
            MethodInfo method = type.GetMethod("TryParse", bindingFlags, null, parameterTypes, null);

            return method;
        }

        public int Compare(T x, T y)
        {
            return Comparer<T>.Default.Compare(x, y);
        }

        private void Coerce(ref T value, T lower, T upper)
        {
            if (Compare(value, upper) > 0)
            {
                value = upper;
            }
            if (Compare(value, lower) < 0)
            {
                value = lower;
            }
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
                T newValue = Value;

                Coerce(ref newValue, LowerLimit, UpperLimit);

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
            T origValue = Value;
            T newValue = origValue;
            int numDecades = origValue.ToString().Split(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator.ToCharArray())[0].Length;
            int decade = numDecades;

            /* check at which decade the cursor is positioned */
            for (int pos = 0; pos < origText.Length; pos++)
            {
                if (pos < oldSelectionStart && (char.IsDigit(origText[pos]) || origText[pos] == '-'))
                {
                    decade--;
                }
            }

            /* add/subtract the mouse wheel actions */
            Add(ref newValue, Math.Sign(e.Delta), decade);
            //newValue += Math.Sign(e.Delta) * ((T)Math.Pow(10, (double)decade));

            Coerce(ref newValue,LowerLimit, UpperLimit);

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

        protected virtual void Add(ref T newValue, int sign, int decade)
        {
        }

        public T Value
        {
            get
            {
                T value;

                if (TryParse(Text, out value))
                {
                    return value;
                }
                return default(T);
            }
            set
            {
                Text = value.ToString();
            }
        }
    }
}
