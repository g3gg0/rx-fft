using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using LibRXFFT.Libraries.USB_RX.Misc;
using System.Collections;
using LibRXFFT.Libraries.Misc;

namespace LibRXFFT.Components.GDI
{
    public partial class AD6636FilterList : UserControl
    {
        public long NCOFreq = 0;
        public event EventHandler FilterSelected;

        private ArrayList FilterFiles = new ArrayList();
        private Button LastButton = null;
        private Color LastButtonColor;


        public AD6636FilterList()
        {
            InitializeComponent();
        }

        public AD6636FilterList(string path, long NCOFreq)
        {
            this.NCOFreq = NCOFreq;
            InitializeComponent();
            UpdateFilters(path, NCOFreq);
        }
        
        public void UpdateFilters(string path)
        {
            flowLayout.Controls.Clear();
            FilterFiles.Clear();

            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] files = di.GetFiles("*.f36");

            foreach (FileInfo fi in files)
            {
                AD6636FilterFile filter = new AD6636FilterFile(fi.FullName);
                if (filter.Valid && filter.InputFrequency == NCOFreq)
                {
                    FilterFiles.Add(filter);
                }
            }

            FilterFiles.Sort();

            foreach (AD6636FilterFile filter in FilterFiles)
            {
                AddFilter(filter);
            }
        }

        public void UpdateFilters(string path, long NCOFreq)
        {
            this.NCOFreq = NCOFreq;
            UpdateFilters(path);
        }

        void AddFilter(AD6636FilterFile filter)
        {
            Button btn = new Button();

            btn.Text = FrequencyFormatter.FreqToString(filter.Width).Replace("Hz", "");
            btn.Margin = new Padding(1, 1, 0, 0);
            btn.Size = new Size(50, 20);
            btn.FlatStyle = FlatStyle.Popup;
            btn.Click += new EventHandler(delegate(object sender, EventArgs e)
            {
                if (LastButton != null)
                    LastButton.ForeColor = LastButtonColor;

                LastButton = btn;
                LastButtonColor = LastButton.ForeColor;
                LastButton.ForeColor = Color.Red;
                if (FilterSelected != null)
                    FilterSelected(filter, null);
            });
            btn.MouseUp +=new MouseEventHandler(delegate(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Right)
                {
                    string msg = "";

                    msg += "  Filter details:" + Environment.NewLine;
                    msg += "-------------------------------------" + Environment.NewLine;
                    msg += "  Version: " + filter.ProgramVersion + Environment.NewLine;
                    msg += "  Device : " + filter.DeviceName + Environment.NewLine;
                    msg += "  Rate   : " + FrequencyFormatter.FreqToStringAccurate(filter.OutputFrequency) + Environment.NewLine;
                    msg += "  Width  : " + FrequencyFormatter.FreqToStringAccurate(filter.Width) + Environment.NewLine;
                    string[] nameParts = filter.FileName.Split('\\');
                    if (nameParts.Length > 0)
                    {
                        msg += "  Filename: "  + nameParts[nameParts.Length-1] + Environment.NewLine;
                    }
                    else
                    {
                        msg += "  Filename: "  + filter.FileName + Environment.NewLine;
                    }
                    MessageBox.Show(msg);
                }
            });
            flowLayout.Controls.Add(btn);
        }

    }
}
