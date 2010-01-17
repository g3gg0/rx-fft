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
        private LinkedList<FrequencyMarker> Markers;

        private ListViewItem[] ListItems;
        private Dictionary<ListViewItem, FrequencyMarker> ItemMarkerMap;

        public MainScreen.delegateGetTuner GetTuner;
        public event EventHandler MarkersChanged;

        public MarkerListDialog(LinkedList<FrequencyMarker> markers)
        {
            InitializeComponent();

            this.Markers = markers;

            UpdateMarkerListInternal();
        }

        delegate void delegateUpdateMarkerList();

        public void UpdateMarkerList()
        {
            BeginInvoke(new delegateUpdateMarkerList(UpdateMarkerListInternal));
        }

        private void UpdateMarkerListInternal()
        {
            FrequencyMarker[] markers = Markers.ToArray();

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

        private void UpdateMarkerTreeInternal()
        {

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

            Markers.AddLast(marker);

            UpdateMarkerListInternal();

            if (MarkersChanged != null)
            {
                MarkersChanged(null, null);
            }
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
                    UpdateMarkerListInternal();

                    if (MarkersChanged != null)
                    {
                        MarkersChanged(null, null);
                    }
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

                    Markers.Remove(marker);

                    UpdateMarkerListInternal();

                    if (MarkersChanged != null)
                    {
                        MarkersChanged(null, null);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void LoadList(string file)
        {
            TextReader reader = new StreamReader(file);
            XmlSerializer serializer = new XmlSerializer(typeof(FrequencyMarker[]));
            FrequencyMarker[] markers = (FrequencyMarker[])serializer.Deserialize(reader);

            if (markers != null)
            {
                Markers.Clear();
                foreach (FrequencyMarker marker in markers)
                {
                    Markers.AddLast(marker);
                }

                UpdateMarkerListInternal();

                if (MarkersChanged != null)
                {
                    MarkersChanged(null, null);
                }
            }
        }

        private void SaveList(string file)
        {
            TextWriter writer = new StreamWriter(file);
            FrequencyMarker[] markers = Markers.ToArray<FrequencyMarker>();
            XmlSerializer serializer = new XmlSerializer(typeof(FrequencyMarker[]));

            serializer.Serialize(writer, markers);
            writer.Close();
        }

        private void loadListMenu_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    LoadList(dlg.FileName);
                }
            }
            catch (Exception ex)
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
                    SaveList(dlg.FileName);
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void clearListMenu_Click(object sender, EventArgs e)
        {
            Markers.Clear();
            UpdateMarkerListInternal();
        }


    }
}
