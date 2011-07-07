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
            sdcch.L1BurstIGet( ref l1bursts);

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
            sdcch.L1BurstIGet(ref l1bursts);

            /* crypt it */
            byte[] kc = new byte[8];
            ByteUtil.BytesFromString ( textKc.Text, ref kc );

            GSMParameters param = new GSMParameters();
            param.FN = uint.Parse(textFN.Text);

            CryptA5 A5Algo = new CryptA5(kc);

            if (radioSameFN.Checked)
            {
                A5Algo.CryptDownlink(l1bursts[0], param.Count);
                A5Algo.CryptDownlink(l1bursts[1], param.Count);
                A5Algo.CryptDownlink(l1bursts[2], param.Count);
                A5Algo.CryptDownlink(l1bursts[3], param.Count);
            }
            else
            {
                A5Algo.CryptDownlink(l1bursts[0], param.Count);
                param.FN++;
                A5Algo.CryptDownlink(l1bursts[1], param.Count);
                param.FN++;
                A5Algo.CryptDownlink(l1bursts[2], param.Count);
                param.FN++;
                A5Algo.CryptDownlink(l1bursts[3], param.Count);
            }

            textL1crypt0.Text = ByteUtil.BitsToString(l1bursts[0]);
            textL1crypt1.Text = ByteUtil.BitsToString(l1bursts[1]);
            textL1crypt2.Text = ByteUtil.BitsToString(l1bursts[2]);
            textL1crypt3.Text = ByteUtil.BitsToString(l1bursts[3]);
        }

        private void btnBoolGadConvert_Click(object sender, EventArgs e)
        {
            bool inputError = false;
            bool dirBoolToGad = radioBoolToGad.Checked;

            if (dirBoolToGad)
            {
                if (textBool0.Text.Length != 114 || textBool1.Text.Length != 114
                    || textBool2.Text.Length != 114 || textBool3.Text.Length != 114 )
                    inputError = true;
                MessageBox.Show("not supported yet!");
                return;
            }
            else
            {
                byte[] byteBuf = new byte[19];

                GSMParameters param = new GSMParameters();
                SDCCHBurst sdcch = new SDCCHBurst();

                ByteUtil.BytesFromString(textGad0.Text, ref byteBuf);
                sdcch.ParseData(param, ByteUtil.BitsFromBytes(byteBuf), 0 );

                ByteUtil.BytesFromString(textGad1.Text, ref byteBuf);
                sdcch.ParseData(param, ByteUtil.BitsFromBytes(byteBuf), 1);

                ByteUtil.BytesFromString(textGad2.Text, ref byteBuf);
                sdcch.ParseData(param, ByteUtil.BitsFromBytes(byteBuf), 2);

                ByteUtil.BytesFromString(textGad3.Text, ref byteBuf);
                sdcch.ParseData(param, ByteUtil.BitsFromBytes(byteBuf), 3);

                bool[][] boolBuf = new bool[4][];
                for ( int i = 0; i < boolBuf.Length; i++ )
                    boolBuf[i] = new bool[114];

                sdcch.L1BurstEGet(ref boolBuf);

                if (boolBuf == null)
                {
                    MessageBox.Show("error reading bits");
                    return;
                }

                textBool0.Text = ByteUtil.BitsToString(boolBuf[0]);
                textBool1.Text = ByteUtil.BitsToString(boolBuf[1]);
                textBool2.Text = ByteUtil.BitsToString(boolBuf[2]);
                textBool3.Text = ByteUtil.BitsToString(boolBuf[3]);


            }

        }
    }
}
