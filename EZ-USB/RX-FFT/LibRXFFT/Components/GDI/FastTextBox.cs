using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace LibRXFFT.Components.GDI
{
    public partial class FastTextBox : UserControl
    {
        private ArrayList TextLines = new ArrayList();
        private StringBuilder WholeText = new StringBuilder();
        private string CurrentLine = "";
        private int NextLineToAdd = 0;
        private int StartLine = 0;
        private int VisibleLines = 0;
        private int ScrollPos = 0;
        private int ChangeTimeout = 0;
        private Timer UpdateTimer = new Timer();
        private bool StickBottom = true;
        private bool Changed = true;
        private bool FastMode = false;

        /* switch to fast mode when more than this amount of characters have to be displayed */
        private int FastModeMargin = 100000;

        public FastTextBox()
        {
            InitializeComponent();

            scrollBar.Value = 1000;

            textBox1.MouseWheel += new MouseEventHandler(textBox1_MouseWheel);

            UpdateTimer.Interval = 100;
            UpdateTimer.Tick += new EventHandler(UpdateTimer_Tick);
            UpdateTimer.Start();
        }

        private void SetFastMode(bool enabled)
        {
            FastMode = enabled;

            if (!FastMode)
            {
                scrollBar.Width = 0;
                UpdateTextBoxSize();
                textBox1.ScrollBars = ScrollBars.Vertical;
            }
            else
            {
                scrollBar.Width = 20;
                UpdateTextBoxSize();
                textBox1.ScrollBars = ScrollBars.None;
            }

        }

        public override string Text
        {
            get
            { 
                return WholeText.ToString(); 
            }
            set
            {
                Clear();
                AppendText(value);
            }
        }

        void UpdateTimer_Tick(object sender, EventArgs e)
        {
            UpdateText();
        }

        private void FastTextBox_SizeChanged(object sender, EventArgs e)
        {
            lock (this)
            {
                UpdateTextBoxSize();

                int fontHeight = TextRenderer.MeasureText("^_^", textBox1.Font).Height;
                VisibleLines = (int)((double)Size.Height / (double)fontHeight);

                Changed = true;
                UpdateText();
            }
        }

        public void Clear()
        {
            lock (this)
            {
                StartLine = 0;
                ChangeTimeout = 0;
                ScrollPos = 0;
                NextLineToAdd = 0;
                CurrentLine = "";
                Changed = true;
                WholeText.Length = 0;
                Invoke(new Action(() => { scrollBar.Value = 1000; TextLines.Clear(); }));

                UpdateText();
            }
        }

        public void AppendText(string text)
        {
            lock (this)
            {
                WholeText.Append(text);

                /* append to current line */
                CurrentLine += text;

                string[] lines = CurrentLine.Split('\n');

                /* no full line yet? */
                if (lines.Length < 2)
                {
                    return;
                }

                /* skip the last line if its not completed yet */
                if (lines[lines.Length - 1].Length != 0)
                {
                    CurrentLine = lines[lines.Length - 1];
                    lines[lines.Length - 1] = null;
                }
                else
                {
                    CurrentLine = "";
                }

                /* add all (full) lines to the list */
                foreach (string line in lines)
                {
                    if (line != null)
                    {
                        /* remove \r */
                        TextLines.Add(line.Replace("\r", ""));
                    }
                }

                if (StickBottom)
                {
                    int newLine = TextLines.Count - VisibleLines;
                    newLine = Math.Max(0, newLine);
                    newLine = Math.Min(TextLines.Count, newLine);

                    StartLine = newLine;
                }

                Changed = true;
            }
        }


        void textBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            lock (this)
            {
                if (!FastMode)
                {
                    return;
                }

                int newLine = StartLine;

                if (e.Delta < 0)
                {
                    newLine++;
                }
                else
                {
                    newLine--;
                }

                newLine = Math.Min(TextLines.Count - VisibleLines, newLine);
                newLine = Math.Max(0, newLine);

                StartLine = newLine;

                Changed = true;

                UpdateText();
            }
        }


        private void scrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            lock (this)
            {
                if (!FastMode)
                {
                    return;
                }

                int newPos = scrollBar.Value;
                int delta = newPos - ScrollPos;
                int newLine = StartLine;
                int lines = TextLines.Count;

                /* arrow clicked? */
                if (Math.Abs(delta) == 1)
                {
                    if (delta > 0)
                    {
                        newLine++;
                    }
                    else
                    {
                        newLine--;
                    }
                }
                else
                {
                    newLine = (int)((((double)newPos) / 1000.0f) * lines);
                }

                /* calculate new scroll bar position value */
                newPos = (int)(((double)newLine / (double)lines) * 1000.0f);


                /* scrolled down to the last 5% */
                if (delta > 0 && newPos > 950)
                {
                    StickBottom = true;
                    newPos = 1000;
                    newLine = lines - VisibleLines;
                }
                else
                {
                    StickBottom = false;
                }

                newLine = Math.Min(lines - VisibleLines, newLine);
                newLine = Math.Max(0, newLine);

                newPos = Math.Max(0, newPos);
                newPos = Math.Min(1000, newPos);

                scrollBar.Value = newPos;
                ScrollPos = newPos;
                StartLine = newLine;

                Changed = true;

                UpdateText();
            }
        }

        private void UpdateText()
        {
            /* when one line or less is visible, display nothing */
            if (VisibleLines <= 1)
            {
                return;
            }

            lock (this)
            {
                if (!Changed)
                {
                    /* we are already in normal mode, no need to check */
                    if (!FastMode)
                    {
                        return;
                    }

                    /* check for "fast mode" timeout */
                    if (ChangeTimeout <= 50)
                    {
                        ChangeTimeout++;
                    }

                    if (ChangeTimeout == 50)
                    {
                        SetFastMode(false);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    ChangeTimeout = 0;
                    if (WholeText.Length > FastModeMargin)
                    {
                        SetFastMode(true);
                    }
                }

                Changed = false;

                if (!FastMode)
                {
                    textBox1.Text = WholeText.ToString();
                    textBox1.SelectionStart = GetFirstShownCharPos();
                    textBox1.ScrollToCaret();
                }
                else
                {
                    bool first = true;
                    int start = StartLine;
                    int end = start + VisibleLines;
                    StringBuilder builder = new StringBuilder();

                    for (int num = start; num < end; num++)
                    {
                        if (num < TextLines.Count)
                        {
                            if (!first)
                            {
                                builder.Append(Environment.NewLine);
                            }
                            first = false;
                            builder.Append(TextLines[num]);
                        }
                    }

                    textBox1.Text = builder.ToString();
                }
            }
        }

        private int GetFirstShownCharPos()
        {
            int start = 0;
            int end = StartLine;
            int length = 0;

            for (int num = start; num < end; num++)
            {
                length += ((string)TextLines[num]).Length + 2;
            }

            return length;
        }

        private void UpdateTextBoxSize()
        {
            int width = Size.Width - scrollBar.Width - 3;
            int height = Size.Height;

            textBox1.Size = new Size(width, height);
        }
    }
}
