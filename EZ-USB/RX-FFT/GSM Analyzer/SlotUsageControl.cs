using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibRXFFT.Libraries.GSM.Layer1.Bursts;
using LibRXFFT.Components.GDI;
using LibRXFFT.Libraries;

namespace GSM_Analyzer
{
    public partial class SlotUsageControl : UserControl
    {
        private Dictionary<NormalBurst, StatusLamp> BurstLampMap = new Dictionary<NormalBurst, StatusLamp>();
        private Dictionary<NormalBurst, string> BurstStatusMap = new Dictionary<NormalBurst, string>();
        private NormalBurst CurrentHoveredBurst = null;

        public SlotUsageControl()
        {
            InitializeComponent();
        }

        public void UpdateSlots(LinkedList<NormalBurst> bursts)
        {
            CleanupLamps(bursts);

            foreach (NormalBurst burst in bursts)
            {
                StatusLamp lamp = GetLamp(burst);
                string status = "---";

                switch (burst.State)
                {
                    case NormalBurst.eBurstState.Idle:
                        status = "Idle";
                        lamp.State = eLampState.Grayed;
                        break;

                    case NormalBurst.eBurstState.PlainTraffic:
                        status = "not encrypted";
                        lamp.State = eLampState.Green;
                        break;

                    case NormalBurst.eBurstState.CryptedTraffic:
                        status = "encrypted";
                        lamp.State = eLampState.Red;
                        break;

                    case NormalBurst.eBurstState.DecryptedTraffic:
                        status = "encrypted (Kc: " + ByteUtil.BytesToString(burst.A5CipherKey) + ")";
                        lamp.State = eLampState.Yellow;
                        break;
                }

                lock (BurstLampMap)
                {
                    BurstStatusMap[burst] = status;
                }

                lock (lblDetails)
                {
                    if (CurrentHoveredBurst == burst)
                    {
                        string msg = "[" + burst.Name + "], TS: " + burst.TimeSlot + ", " + BurstStatusMap[burst];

                        BeginInvoke(new Action(() =>
                        {
                            lblDetails.Text = msg;
                        }));
                    }
                }
            }
        }

        private void CleanupLamps(LinkedList<NormalBurst> bursts)
        {
            LinkedList<KeyValuePair<NormalBurst, StatusLamp>> removePairs = new LinkedList<KeyValuePair<NormalBurst, StatusLamp>>();

            lock (BurstLampMap)
            {
                foreach (KeyValuePair<NormalBurst, StatusLamp> pair in BurstLampMap)
                {
                    /* burst does not exist anymore - remove lamp */
                    if (!bursts.Contains(pair.Key))
                    {
                        removePairs.AddLast(pair);
                    }
                }
            }

            BeginInvoke(new Action(() =>
            {
                /* remove all lamps that are not used anymore */
                foreach (KeyValuePair<NormalBurst, StatusLamp> pair in removePairs)
                {
                    lock (BurstLampMap)
                    {
                        BurstLampMap.Remove(pair.Key);
                        BurstStatusMap.Remove(pair.Key);
                    }

                    if (pair.Key is TCHBurst)
                    {
                        panelTCH.Controls.Remove(pair.Value);
                    }

                    if (pair.Key is SDCCHBurst)
                    {
                        panelSDCCH.Controls.Remove(pair.Value);
                    }
                }
            }));
        }

        private StatusLamp GetLamp(NormalBurst burst)
        {
            lock (BurstLampMap)
            {
                if (BurstLampMap.ContainsKey(burst))
                {
                    return BurstLampMap[burst];
                }
            }

            StatusLamp lamp = new StatusLamp();
            lamp.Margin = new Padding(1);
            lamp.Width = 20;
            lamp.Height = 20;

            lamp.MouseEnter += (object sender, EventArgs e) =>
            {
                lock (lblDetails)
                {
                    CurrentHoveredBurst = burst;
                    lblDetails.Text = "[" + burst.Name + "], TS: " + burst.TimeSlot + ", " + BurstStatusMap[burst];
                }
            };

            lamp.MouseLeave += (object sender, EventArgs e) =>
            {
                lock (lblDetails)
                {
                    CurrentHoveredBurst = null;
                    lblDetails.Text = "---";
                }
            };

            lock (BurstLampMap)
            {
                BurstStatusMap.Add(burst, "---");
                BurstLampMap.Add(burst, lamp);
            }

            RebuildLamps();

            return lamp;
        }

        private void RebuildLamps()
        {
            BeginInvoke(new Action(() =>
            {
                try
                {
                    SortedList<string, StatusLamp> lampsSDCCH = new SortedList<string, StatusLamp>();
                    SortedList<string, StatusLamp> lampsTCH = new SortedList<string, StatusLamp>();

                    lock (BurstLampMap)
                    {
                        foreach (KeyValuePair<NormalBurst, StatusLamp> pair in BurstLampMap)
                        {
                            string sortOrder = pair.Key.TimeSlot + pair.Key.Name;

                            if (pair.Key is TCHBurst)
                            {
                                lampsTCH.Add(sortOrder, pair.Value);
                            }

                            if (pair.Key is SDCCHBurst)
                            {
                                lampsSDCCH.Add(sortOrder, pair.Value);
                            }
                        }
                    }

                    panelSDCCH.SuspendLayout();
                    panelTCH.SuspendLayout();

                    panelSDCCH.Controls.Clear();
                    panelTCH.Controls.Clear();

                    foreach (KeyValuePair<string, StatusLamp> pair in lampsSDCCH)
                    {
                        panelSDCCH.Controls.Add(pair.Value);
                    }
                    foreach (KeyValuePair<string, StatusLamp> pair in lampsTCH)
                    {
                        panelTCH.Controls.Add(pair.Value);
                    }

                    panelSDCCH.ResumeLayout();
                    panelTCH.ResumeLayout();
                }
                catch (Exception e)
                {
                }

            }));
        }
    }
}
