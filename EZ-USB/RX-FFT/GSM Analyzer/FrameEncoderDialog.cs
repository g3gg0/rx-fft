using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using LibRXFFT.Libraries.GSM.Layer1.Bursts;
using LibRXFFT.Libraries;
using LibRXFFT.Libraries.GSM.Layer1;

namespace GSM_Analyzer
{
    public partial class FrameEncoderDialog : Form
    {
        public FrameEncoderDialog()
        {
            InitializeComponent();
        }

        private void btnL2ToL1_Click(object sender, EventArgs e)
        {
            byte[] l2data = new byte[23];

            if (textL2Data.Text.Length != 23 * 2 && textL2Data.Text.Length != 23 * 3 - 1)
            {
                MessageBox.Show(this, "Supply 23 bytes L2 data ...");
                textL1burst0.Text = "";
                textL1burst1.Text = "";
                textL1burst2.Text = "";
                textL1burst3.Text = "";
                return;
            }

            ByteUtil.BytesFromString( textL2Data.Text, ref l2data);

            bool[][] l1bursts = new bool[4][];
            for (int i = 0; i < l1bursts.Length; i++)
                l1bursts[i] = new bool[114];

            SDCCHBurst sdcch = new SDCCHBurst();
            sdcch.L2DataAdd ( l2data );
            sdcch.L2ToL1Convert();
            sdcch.L1BurstGet( ref l1bursts);

            textL1burst0.Text = ByteUtil.BitsToString(l1bursts[0]);
            textL1burst1.Text = ByteUtil.BitsToString(l1bursts[1]);
            textL1burst2.Text = ByteUtil.BitsToString(l1bursts[2]);
            textL1burst3.Text = ByteUtil.BitsToString(l1bursts[3]);
            
        }

        private void btnEncryptToL1_Click(object sender, EventArgs e)
        {
            bool error = false;

            if (textL2Data.Text.Length != 23 * 2 && textL2Data.Text.Length != 23 * 3 - 1)
            {
                MessageBox.Show(this, "Supply 23 bytes L2 data ...");
                error = true;
            }

            if (textKc.Text.Length != 8 * 2)
            {
                MessageBox.Show(this, "Supply 8 bytes Kc ...");
                error = true;
            }

            if (textFN.Text.Length == 0)
            {
                MessageBox.Show(this, "Supply frame number ...");
                error = true;
            }

            if (error)
            {
                textL1crypt0.Text = "";
                textL1crypt1.Text = "";
                textL1crypt2.Text = "";
                textL1crypt3.Text = "";
                return;
            }

            byte[] l2data = new byte[23];

            ByteUtil.BytesFromString(textL2Data.Text, ref l2data);

            bool[][] l1bursts = new bool[4][];
            for (int i = 0; i < l1bursts.Length; i++)
                l1bursts[i] = new bool[114];

            SDCCHBurst sdcch = new SDCCHBurst();
            sdcch.L2DataAdd(l2data);
            sdcch.L2ToL1Convert();
            sdcch.L1BurstGet(ref l1bursts);

            /* crypt it */
            byte[] kc = new byte[8];
            ByteUtil.BytesFromString ( textKc.Text, ref kc );

            GSMParameters param = new GSMParameters();
            param.FN = uint.Parse(textFN.Text);

            CryptA5 A5Algo = new CryptA5(kc);
            A5Algo.CryptDownlink(l1bursts[0], param.Count);
            A5Algo.CryptDownlink(l1bursts[1], param.Count);
            A5Algo.CryptDownlink(l1bursts[2], param.Count);
            A5Algo.CryptDownlink(l1bursts[3], param.Count);

            textL1crypt0.Text = ByteUtil.BitsToString(l1bursts[0]);
            textL1crypt1.Text = ByteUtil.BitsToString(l1bursts[1]);
            textL1crypt2.Text = ByteUtil.BitsToString(l1bursts[2]);
            textL1crypt3.Text = ByteUtil.BitsToString(l1bursts[3]);
        }
    }
}
