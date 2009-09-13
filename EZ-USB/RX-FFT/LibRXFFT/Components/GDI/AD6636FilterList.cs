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
        ArrayList FilterFiles = new ArrayList();

        public event EventHandler FilterSelected;

        public AD6636FilterList()
        {
            InitializeComponent();
        }

        public AD6636FilterList(string path, long NCOFreq)
        {
            InitializeComponent();
            UpdateFilters(path, NCOFreq);
        }
        

        public void UpdateFilters(string path, long NCOFreq)
        {
            flowLayout.Controls.Clear();
            FilterFiles.Clear();

            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] files = di.GetFiles("*.f36");

            foreach (FileInfo fi in files)
            {
                FilterFile filter = new FilterFile(fi.FullName);
                if (filter.Valid && filter.InputFrequency == NCOFreq)
                {
                    FilterFiles.Add(filter);
                }
            }

            foreach (FilterFile filter in FilterFiles)
            {
                AddFilter(filter);
            }
        }

        void AddFilter(FilterFile filter)
        {
            Button btn = new Button();

            btn.Text = FrequencyFormatter.FreqToString(filter.Width).Replace("Hz", "");
            btn.Margin = new Padding(1, 1, 0, 0);
            btn.Size = new Size(50, 20);
            btn.FlatStyle = FlatStyle.Popup;
            btn.Click += new EventHandler(delegate(object sender, EventArgs e)
            {
                if (FilterSelected != null)
                    FilterSelected(filter, null);
            });
            flowLayout.Controls.Add(btn);
        }

    }
}
