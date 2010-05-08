using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;
using LibRXFFT.Components.DirectX;
using LibRXFFT.Libraries.Misc;
using LibRXFFT.Libraries.USB_RX.Tuners;

namespace RX_FFT.Dialogs
{
    public partial class MarkerListDialog : Form
    {
        private FrequencyMarkerList MarkerList;
        private ListViewItem[] ListItems;
        private Dictionary<ListViewItem, FrequencyMarker> ItemMarkerMap;

        public MainScreen.delegateGetTuner GetTuner;

        public MarkerListDialog(FrequencyMarkerList markerList)
        {
            InitializeComponent();

            MarkerList = markerList;
            MarkerList.MarkersChanged += MarkerList_MarkersChanged;

            UpdateMarkerList();
        }

        private void UpdateMarkerList()
        {
            FrequencyMarker[] markers = MarkerList.Markers.ToArray();

            if (ListItems == null || ListItems.Length != markers.Length)
            {
                ListItems = new ListViewItem[markers.Length];
                for (int pos = 0; pos < ListItems.Length; pos++)
                    ListItems[pos] = new ListViewItem(new string[2]);

                lstMarkers.Items.Clear();
                lstMarkers.Items.AddRange(ListItems);
            }

            ItemMarkerMap = new Dictionary<ListViewItem, FrequencyMarker>();

            for (int pos = 0; pos < ListItems.Length; pos++)
            {
                ItemMarkerMap.Add(ListItems[pos], markers[pos]);
                ListItems[pos].SubItems[0].Text = FrequencyFormatter.FreqToStringAccurate(markers[pos].Frequency);
                ListItems[pos].SubItems[1].Text = markers[pos].Label;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            MarkerList.MarkersChanged -= MarkerList_MarkersChanged;
            base.OnClosed(e);
        }

        void MarkerList_MarkersChanged(object sender, EventArgs e)
        {
            UpdateMarkerList();
        }

        void lstMarkers_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                ListView.SelectedListViewItemCollection selectedMarkers = lstMarkers.SelectedItems;

                if (selectedMarkers.Count == 1)
                {
                    FrequencyMarker marker = ItemMarkerMap[selectedMarkers[0]];

                    Tuner tuner = GetTuner();

                    if (tuner != null)
                    {
                        tuner.SetFrequency(marker.Frequency);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void newEntryMenu_Click(object sender, EventArgs e)
        {
            long freq = 0;
            Tuner tuner = GetTuner();

            if (tuner != null)
            {
                freq = tuner.GetFrequency();
            }

            FrequencyMarker marker = new FrequencyMarker(freq);
            MarkerDetailsDialog dlg = new MarkerDetailsDialog("Add Marker...", marker);

            if (dlg.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            MarkerList.Add(marker);
        }

        private void editEntryMenu_Click(object sender, EventArgs e)
        {
            try
            {
                ListView.SelectedListViewItemCollection selectedMarkers = lstMarkers.SelectedItems;

                if (selectedMarkers.Count == 1)
                {
                    FrequencyMarker marker = ItemMarkerMap[selectedMarkers[0]];

                    MarkerDetailsDialog dlg = new MarkerDetailsDialog(marker);

                    dlg.ShowDialog();
                }
            }
            catch (Exception)
            {
            }
        }

        private void deleteEntryMenu_Click(object sender, EventArgs e)
        {
            try
            {
                ListView.SelectedListViewItemCollection selectedMarkers = lstMarkers.SelectedItems;

                if (selectedMarkers.Count == 1)
                {
                    FrequencyMarker marker = ItemMarkerMap[selectedMarkers[0]];

                    MarkerList.Remove(marker);
                }
            }
            catch (Exception)
            {
            }
        }

        private void loadListMenu_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    MarkerList.Load(dlg.FileName);
                }
            }
            catch (Exception)
            {
            }
        }

        private void saveListMenu_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    MarkerList.Save(dlg.FileName);
                }
            }
            catch (Exception)
            {
            }
        }

        private void clearListMenu_Click(object sender, EventArgs e)
        {
            MarkerList.Clear();
        }
    }
}
