using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibRXFFT.Libraries;

using LibRXFFT.Libraries.GSM.Layer1;

namespace GSM_Analyzer
{
    public partial class KrakenTestDialog : Form
    {
        GSMAnalyzer Analyzer;

        public KrakenTestDialog(GSMAnalyzer analyzer)
        {
            Analyzer = analyzer;

            InitializeComponent();
        }

        private bool[] generateStream(byte[] kc, uint count)
        {
            bool[] keytsream = new bool[114];

            CryptA5 A5Algo = new CryptA5(kc);
            A5Algo.Count = count;
            A5Algo.RunAlgo();

            Array.Copy(A5Algo.DownlinkKey, keytsream, keytsream.Length);

            return keytsream;

        }

        private void btnRunTest_Click(object sender, EventArgs e)
        {

            if (textBursts.Text.Length == 0 || textTestCount.Text.Length == 0)
            {
                MessageBox.Show("Set >test count< and >bursts per test< first.");
                return;
            }

            krakenWorker.RunWorkerAsync();

            btnRunTest.Enabled = false;

            return;

        }

 

        private void krakenWorker_DoWork(object sender, DoWorkEventArgs e)
        {

            bool[] key1 = null;
            bool[] key2 = null;
            uint count1, count2;

            uint testCount, burstPerTest;
            uint success = 0, bursts = 0;

            testCount = uint.Parse ( textTestCount.Text );
            burstPerTest = uint.Parse ( textBursts.Text );

            GSMParameters param = new GSMParameters();

            byte[] kc, kc_calculated = null;

            kc = new byte[8];

            Random rand = new Random();

            KrakenClient kraken = new KrakenClient(Analyzer.KrakenHostAddress);

            if (!kraken.Connect())
            {
                MessageBox.Show("Could not connect to kraken.");
                return;
            }

            for (uint i = 0; i < testCount; i++)
            {
                /* get new Kc */
                rand.NextBytes(kc);

                kc_calculated = null;

                /* set framenumber for first burst */
                param.FN = (uint)rand.Next(2715648);

                for (uint j = 0; j < burstPerTest && kc_calculated == null; j++)
                {
                    count1 = param.Count;
                    param.FN += 1;
                    count2 = param.Count;

                    key1 = generateStream(kc, count1);
                    key2 = generateStream(kc, count2);
                    kc_calculated = kraken.RequestResult(key1, count1, key2, count2);
                    if (kc_calculated != null)
                    {
                        /* check if Kc are the same */
                        for (int k = 0; k < kc.Length; k++)
                        {
                            if (kc[k] != kc_calculated[k])
                                break;

                            if (k == 7)
                                success++;

                        }

                    }
                    bursts++;
                    krakenWorker.ReportProgress(((int)i * (int)burstPerTest + (int)j + 1) * 100 / ((int)burstPerTest * (int)testCount));

                }
            }

            MessageBox.Show("Tried to crack " + bursts + " bursts.\nSuccessfully cracked " + success + " sessions.\nCoverage is " + (success * 100 / testCount ) + "%.");


        }

        private void krakenWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnRunTest.Enabled = true;
        }

        private void krakenWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            krakenTestProgress.Value = e.ProgressPercentage;
        }
    }
}
