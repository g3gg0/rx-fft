using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibRXFFT.Libraries.USB_RX.Tuners;

namespace GSM_Analyzer
{
    public partial class StationListDialog : Form
    {
        private RadioChannelHandler Handler;
        
        public StationListDialog(RadioChannelHandler handler)
        {
            InitializeComponent();
            Handler = handler;
        }

        public void Clear()
        {
            BeginInvoke(new MethodInvoker(() => lstStations.Clear()));
        }

        public void AddStation(long channel, string bsic, string mccMnc, string lac, string cellIdent, string cbch, string strength)
        {
            ListViewItem item = new ListViewItem(new[] {channel.ToString(), bsic, mccMnc, lac, cellIdent, cbch, strength});
            item.Tag = channel;
            BeginInvoke(new MethodInvoker(() => lstStations.Items.Add(item)));
        }


        private void lstStations_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                ListView.SelectedListViewItemCollection selectedMarkers = lstStations.SelectedItems;

                if (selectedMarkers.Count == 1)
                {
                    Handler.Channel = (long)selectedMarkers[0].Tag;
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
