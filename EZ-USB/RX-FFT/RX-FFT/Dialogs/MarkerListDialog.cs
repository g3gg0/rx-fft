using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;
using LibRXFFT.Libraries.Misc;
using LibRXFFT.Libraries.USB_RX.Tuners;
using LibRXFFT.Libraries.SignalProcessing;
using LibRXFFT.Components.GDI;
using FrequencyMarker = LibRXFFT.Components.DirectX.FrequencyMarker;
using System.Drawing;

namespace RX_FFT.Dialogs
{
    public partial class MarkerListDialog : Form
    {
        private FrequencyMarkerList MarkerList;
        private ListViewItem[] ListItems;
        private Dictionary<ListViewItem, FrequencyMarker> ItemMarkerMap;
        private MainScreen MainScreen;

        public MainScreen.delegateGetTuner GetTuner;

        public MarkerListDialog(FrequencyMarkerList markerList, MainScreen mainScreen)
        {
            InitializeComponent();

            MainScreen = mainScreen;

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
                {
                    ListItems[pos] = new ListViewItem(new string[2]);
                }

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

        FrequencyMarker GetSelected()
        {
            try
            {
                ListView.SelectedListViewItemCollection selectedMarkers = lstMarkers.SelectedItems;

                if (selectedMarkers.Count == 1)
                {
                    return ItemMarkerMap[selectedMarkers[0]];
                }
            }
            catch (Exception)
            {
            }

            return null;
        }

        void lstMarkers_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            try
            {
                switch (e.Button)
                {
                    case MouseButtons.Right:
                        FrequencyMarker marker = GetSelected();
                        Tuner tuner = GetTuner();

                        if (marker != null)
                        {
                            ContextMenu contextMenu = new ContextMenu();
                            MenuItem menuItem;
                            menuItem = new MenuItem("Marker: " + marker.Label);
                            menuItem.Enabled = false;
                            contextMenu.MenuItems.Add(menuItem);

                            menuItem = new MenuItem("-");
                            menuItem.Enabled = false;
                            contextMenu.MenuItems.Add(menuItem);

                            menuItem = new MenuItem("Jump to");
                            menuItem.Enabled = (tuner == null);
                            menuItem.Click += (object s, EventArgs a) =>
                            {
                                tuner.SetFrequency(marker.Frequency);
                            };
                            contextMenu.MenuItems.Add(menuItem);

                            menuItem = new MenuItem("-" );
                            menuItem.Enabled = false;
                            contextMenu.MenuItems.Add(menuItem);

                            menuItem = new MenuItem("Edit...");
                            menuItem.Click += (object s, EventArgs a) =>
                            {
                                MarkerDetailsDialog dlg = new MarkerDetailsDialog(marker);
                                dlg.ShowDialog();
                                UpdateMarkerList();
                            };
                            contextMenu.MenuItems.Add(menuItem);

                            menuItem = new MenuItem("Delete");
                            menuItem.Click += (object s, EventArgs a) =>
                            {
                                MarkerList.Remove(marker); 
                                UpdateMarkerList();
                            };
                            contextMenu.MenuItems.Add(menuItem);

                            menuItem = new MenuItem("-");
                            menuItem.Enabled = false;
                            contextMenu.MenuItems.Add(menuItem);


                            bool demodulation = MainScreen.MarkerDemodulators.ContainsKey(marker);
                            AudioDemodulator Demod = null;
                            DemodulationState DemodState = null;

                            if (demodulation)
                            {
                                Demod = MainScreen.MarkerDemodulators[marker];
                                DemodState = Demod.DemodState;

                                menuItem = new MenuItem("Demodulate");
                                menuItem.Checked = true;
                                menuItem.Click += (object s, EventArgs a) =>
                                {
                                    if (DemodState.Dialog != null)
                                    {
                                        DemodState.Dialog.Close();
                                        DemodState.Dialog = null;
                                    }
                                    Demod.Stop();
                                    Demod.Close();

                                    MainScreen.MarkerDemodulators.Remove(marker);
                                };
                                contextMenu.MenuItems.Add(menuItem);

                                menuItem = new MenuItem("Demodulation Options...");
                                menuItem.Checked = (DemodState.Dialog != null);
                                menuItem.Click += (object s, EventArgs a) =>
                                {
                                    if (DemodState.Dialog != null)
                                    {
                                        DemodState.Dialog.Close();
                                        DemodState.Dialog = null;
                                    }
                                    else
                                    {
                                        DemodState.Dialog = new DemodulationDialog(DemodState);
                                        DemodState.Dialog.FrequencyFixed = true;
                                        DemodState.Dialog.Show();
                                    }
                                };

                                contextMenu.MenuItems.Add(menuItem);
                            }
                            else
                            {
                                menuItem = new MenuItem("Demodulate");
                                menuItem.Click += (object s, EventArgs a) =>
                                {
                                    Demod = new AudioDemodulator();
                                    DemodState = Demod.DemodState;
                                    DemodState.BaseFrequency = tuner.GetFrequency();
                                    DemodState.DemodulationFrequencyMarker = marker.Frequency;
                                    DemodState.Description = marker.Label;

                                    MainScreen.MarkerDemodulators.Add(marker, Demod);

                                    DemodState.Dialog = new DemodulationDialog(DemodState);
                                    DemodState.Dialog.FrequencyFixed = true;
                                    DemodState.Dialog.Show();
                                    DemodState.Dialog.UpdateInformation();

                                    Demod.Start(MainScreen.AudioShmem);
                                };

                                contextMenu.MenuItems.Add(menuItem);

                                menuItem = new MenuItem("Demodulation Options...");
                                menuItem.Enabled = false;
                                contextMenu.MenuItems.Add(menuItem);
                            }

                            Point popupPos = this.PointToClient(MousePosition);

                            popupPos.X -= 20;
                            popupPos.Y -= 20;
                            contextMenu.Show(this, popupPos);
                        }
                        break;
                }
            }
            catch (Exception)
            {
            }
        }

        void lstMarkers_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                FrequencyMarker marker = GetSelected();

                Tuner tuner = GetTuner();

                if (marker != null && tuner != null)
                {
                    tuner.SetFrequency(marker.Frequency);
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
            UpdateMarkerList();
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
            UpdateMarkerList();
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
            UpdateMarkerList();
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
