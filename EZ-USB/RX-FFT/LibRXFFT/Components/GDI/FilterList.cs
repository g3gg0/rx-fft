using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using LibRXFFT.Libraries.Misc;
using LibRXFFT.Libraries.USB_RX.Misc;

namespace LibRXFFT.Components.GDI
{
    public partial class FilterList : UserControl
    {
        public long NCOFreq = 0;
        public event EventHandler FilterSelected;
        public object FirstFilter = null;

        private ArrayList FilterFiles = new ArrayList();
        private Button LastButton = null;
        private Color LastButtonColor;


        public FilterList()
        {
            InitializeComponent();
            ShowFiles(true);
        }

        public FilterList(string path, long NCOFreq)
        {
            this.NCOFreq = NCOFreq;
            InitializeComponent();
            UpdateFilters(path);
        }

        public void AddFilters(string path)
        {
            try
            {
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
            }
            catch (Exception e)
            {
                //return;
            }

            RebuildFilterFileButtons();
        }

        public void RebuildFilterFileButtons()
        {
            ctrFilterFileButtons.Controls.Clear();
            ctrAtmelFilterButtons.Controls.Clear();

            FilterFiles.Sort();
            FirstFilter = null;

            foreach (AD6636FilterFile filter in FilterFiles)
            {
                AddFilter(filter);
            }

            Button btn = new Button();
            btn.Text = "...";
            btn.Margin = new Padding(1, 1, 0, 0);
            btn.Size = new Size(50, 20);
            btn.FlatStyle = FlatStyle.Popup;
            btn.Click += new EventHandler(delegate(object sender, EventArgs e)
            {
                FolderBrowserDialog d = new FolderBrowserDialog();
                DialogResult result = d.ShowDialog();
                if (result == DialogResult.OK)
                {
                    string folderName = d.SelectedPath;
                    UpdateFilters(folderName);
                }
            });
            ctrFilterFileButtons.Controls.Add(btn);
        }
        
        public void UpdateFilters(string path)
        {
            FilterFiles.Clear();
            AddFilters(path);
        }

        public void FilterSelect(object filter)
        {
            ButtonPressed(filter, null);
        }

        public void ButtonPressed(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            FilterInformation filter = (FilterInformation)btn.Tag;

            if (LastButton != null)
                LastButton.ForeColor = LastButtonColor;

            LastButton = btn;
            LastButtonColor = LastButton.ForeColor;
            LastButton.ForeColor = Color.Red;
            if (FilterSelected != null)
                FilterSelected(filter, null);
        }

        public void AddFilter(FilterInformation filter)
        {
            Button btn = new Button();

            btn.Text = FrequencyFormatter.FreqToString(filter.Width).Replace("Hz", "");
            btn.Margin = new Padding(1, 1, 0, 0);
            btn.Size = new Size(50, 20);
            btn.FlatStyle = FlatStyle.Popup;
            btn.Tag = filter;
            btn.Click += new EventHandler(ButtonPressed);
            btn.MouseUp +=new MouseEventHandler(delegate(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Right)
                {
                    string msg = "";

                    msg += "  Filter details:" + Environment.NewLine;
                    msg += "-------------------------------------" + Environment.NewLine;
                    //msg += "  Version: " + filter.ProgramVersion + Environment.NewLine;
                    //msg += "  Device : " + filter.DeviceName + Environment.NewLine;
                    msg += "  Rate   : " + FrequencyFormatter.FreqToStringAccurate(filter.Rate) + Environment.NewLine;
                    msg += "  Width  : " + FrequencyFormatter.FreqToStringAccurate(filter.Width) + Environment.NewLine;
                    string[] nameParts = filter.Location.Split('\\');
                    if (nameParts.Length > 0)
                    {
                        msg += "  Location:  \\" + nameParts[nameParts.Length - 1] + Environment.NewLine;
                    }
                    else
                    {
                        msg += "  Location:  " + filter.Location + Environment.NewLine;
                    }
                    MessageBox.Show(msg);
                }
            });

            if (FirstFilter == null)
            {
                FirstFilter = btn;
            }

            if (filter is AD6636FilterFile)
            {
                ctrFilterFileButtons.Controls.Add(btn);
            }
            else
            {
                ctrAtmelFilterButtons.Controls.Add(btn);
            }
        }

        public void ShowFiles(bool show)
        {
            ctrFilterFileButtons.Visible = show;
            ctrAtmelFilterButtons.Visible = !show;
        }
    }
}
