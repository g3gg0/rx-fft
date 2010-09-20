// --------------------------------------------------------------------------------------------
// SimExpressForm.cs
// SmartCard Subsembly Express
// Copyright © 2004-2005 Subsembly GmbH
// --------------------------------------------------------------------------------------------

using System;
using System.Drawing;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

using Subsembly.SmartCard;
using Subsembly.SmartCard.PcSc;
using System.Text;
using System.ServiceModel;
using System.Net;

namespace SIMAuthDaemon
{
	/// <summary>
	/// Summary description for SimExpressForm.
	/// </summary>

	public class SIMAuthDaemonForm : Form
	{
		SCardResourceManager m_aCardResourceManager;
        CardDialogsForm m_aCardForm = new CardDialogsForm();
        private System.Windows.Forms.Button readButton;
        private TextBox txtRand;
        private Label label1;
        private TextBox txtLog;
        private SplitContainer splitContainer1;
        private SplitContainer splitContainer2;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

        public delegate void LogMessageEvent(string msg);
        public event LogMessageEvent LogMessage;

        private static SIMAuthDaemonForm DaemonInstance;
        private ServiceHost CommService;

        internal static SIMAuthDaemonForm GetInstance()
        {
            return DaemonInstance;
        }
        
        /// <summary>
		/// 
		/// </summary>
		public SIMAuthDaemonForm()
		{
            DaemonInstance = this;
			InitializeComponent();

			m_aCardResourceManager = new SCardResourceManager();

			// Customize the standard CardExpressForm by setting its properties to our desire.

			m_aCardForm.FooterBackColor = Color.FromArgb(0xF0, 0xF6, 0xFD);

			// First of all we must establish the Smart Card Resource Manager context. Once
			// this is done we must ensure that the context is ultimately released again.

			m_aCardResourceManager.EstablishContext(SCardContextScope.User);

            LogMessage += (string msg) => { BeginInvoke(new Action(() => { txtLog.AppendText(msg + Environment.NewLine); })); };

		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
				if (m_aCardResourceManager != null)
				{
					m_aCardResourceManager.Dispose();
					m_aCardResourceManager = null;
				}
				if (m_aCardForm != null)
				{
					m_aCardForm.Dispose();
					m_aCardForm = null;
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SIMAuthDaemonForm));
            this.readButton = new System.Windows.Forms.Button();
            this.txtRand = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // readButton
            // 
            this.readButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.readButton.Location = new System.Drawing.Point(12, 10);
            this.readButton.Name = "readButton";
            this.readButton.Size = new System.Drawing.Size(96, 23);
            this.readButton.TabIndex = 13;
            this.readButton.Text = "Calc Kc/SRES";
            this.readButton.Click += new System.EventHandler(this.btnReadDetails_Click);
            // 
            // txtRand
            // 
            this.txtRand.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtRand.Location = new System.Drawing.Point(10, 10);
            this.txtRand.Name = "txtRand";
            this.txtRand.Size = new System.Drawing.Size(312, 20);
            this.txtRand.TabIndex = 14;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(114, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 16);
            this.label1.TabIndex = 15;
            this.label1.Text = "Manual RAND:";
            // 
            // txtLog
            // 
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Location = new System.Drawing.Point(0, 0);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.Size = new System.Drawing.Size(527, 147);
            this.txtLog.TabIndex = 14;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.txtLog);
            this.splitContainer1.Size = new System.Drawing.Size(527, 195);
            this.splitContainer1.SplitterDistance = 44;
            this.splitContainer1.TabIndex = 16;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.IsSplitterFixed = true;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.label1);
            this.splitContainer2.Panel1.Controls.Add(this.readButton);
            this.splitContainer2.Panel1.Padding = new System.Windows.Forms.Padding(10);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.txtRand);
            this.splitContainer2.Panel2.Padding = new System.Windows.Forms.Padding(10);
            this.splitContainer2.Size = new System.Drawing.Size(527, 44);
            this.splitContainer2.SplitterDistance = 191;
            this.splitContainer2.TabIndex = 16;
            // 
            // SIMAuthDaemonForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(527, 195);
            this.Controls.Add(this.splitContainer1);
            this.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.SystemColors.WindowText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SIMAuthDaemonForm";
            this.Text = "SIM Auth Daemon";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>

		[STAThread]
		static void Main() 
		{
			Application.EnableVisualStyles();
			Application.DoEvents();

			SIMAuthDaemonForm aSimExpressForm = new SIMAuthDaemonForm();
			Application.Run(aSimExpressForm);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

        SIMAuthDaemon aSim = null;
        private void btnReadDetails_Click(object sender, System.EventArgs e)
		{
            if (aSim == null)
            {
                aSim = _ObtainSim();
                if (aSim == null)
                {
                    return;
                }
            }

			try
			{

                byte[] data = aSim.ReadIccIdentification();
                if (data == null)
                {
                    MessageBox.Show("Error reading GSM SIM card!");
                    _DisposeSim(aSim);
                    aSim = null;
                    return;
                }
                txtLog.AppendText("ICCID: " + CardHex.FromByteArray(data) + Environment.NewLine);


                /* check access to SIM card. fails if CHV required */
                if (aSim.ReadImsi() == null)
                {
                    CardResponseAPDU aRespAPDU = aSim.VerifyChv(this, m_aCardForm,
                        "PIN Verification", "In order to access your SIM the PIN is required.");

                    if (aRespAPDU == null)
                    {
                        // The PIN entry has been cancelled by the user.
                        _DisposeSim(aSim);
                        aSim = null;
                        return;
                    }

                    if (!aRespAPDU.IsSuccessful)
                    {
                        string sHeading = "Failed to verify PIN!";
                        switch (aRespAPDU.SW)
                        {
                            case 0x9804:
                                m_aCardForm.Notify(this, MessageBoxIcon.Warning, sHeading,
                                    "Wrong PIN!");
                                break;
                            case 0x9840:
                                m_aCardForm.Notify(this, MessageBoxIcon.Warning, sHeading,
                                    "Wrong PIN! The SIM has been blocked.");
                                break;
                            case 0x9808:
                                m_aCardForm.Notify(this, MessageBoxIcon.Warning, sHeading,
                                    "The SIM is blocked. Please use mobile phone " +
                                    "in order to unblock the SIM with the PUK.");
                                break;
                            default:
                                m_aCardForm.Notify(this, MessageBoxIcon.Warning, sHeading,
                                    "Unknown reason: (" + aRespAPDU.SW + ")");
                                break;
                        }
                        _DisposeSim(aSim);
                        aSim = null;
                        return;
                    }
                }

                data = aSim.ReadImsi();
                if (data == null)
                {
                    MessageBox.Show("Error reading IMSI!");
                    _DisposeSim(aSim);
                    aSim = null;
                    return;
                }
                txtLog.AppendText("IMSI:  " + CardHex.FromByteArray(data) + Environment.NewLine);

				if (!aSim.SelectDFTelecom())
				{
					MessageBox.Show("Error selecting GSM file!");
                    _DisposeSim(aSim);
                    aSim = null;
					return;
				}

                byte[] rand = CardHex.ToByteArray(txtRand.Text);
                byte[] resp = RunGsmAlgo(rand);

                /**/
			}
			catch(Exception)
			{
				_DisposeSim(aSim);
                aSim = null;
			}
		}

        internal byte[] RunGsmAlgo(byte[] rand)
        {
            if (!aSim.SelectDFTelecom())
            {
                MessageBox.Show("Error reading GSM SIM card!");
                _DisposeSim(aSim);
                aSim = null;
                return null;
            }

            CardResponseAPDU sres = aSim.RunGsmAlgo(rand);

            if (sres != null && sres.GetData() != null)
            {
                txtLog.AppendText("SRES resp " + CardHex.FromByteArray(rand) + "[" + rand.Length + "] -> " + CardHex.FromByteArray(sres.GetData()) + Environment.NewLine);
                return sres.GetData();
            }

            txtLog.AppendText("SRES fail " + CardHex.FromByteArray(rand) + "[" + rand.Length + "] -> <failed> " + CardHex.FromByte(sres.SW1) + " " + CardHex.FromByte(sres.SW2) + Environment.NewLine);
            return null;
        }

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>

		private SIMAuthDaemon _ObtainSim()
		{
            if (CommService == null)
            {
                try
                {
                    CommService = new ServiceHost(typeof(AuthServer));
                    CommService.AddServiceEndpoint(typeof(AuthService), new NetTcpBinding(), "net.tcp://" + Dns.GetHostName() + ":8005");
                    CommService.Open();
                    LogMessage("Created daemon service");
                }
                catch (Exception e)
                {
                    CommService = null;
                    LogMessage("Failed to create service: " + e);
                }
            }

			// Determine the list of readers that are connected to this system. If this
			// list is empty, then bail out.

			ArrayList vsReaderNames = new ArrayList();
            try
            {
                int nReaders = m_aCardResourceManager.ListReaders(vsReaderNames);
                if (nReaders == 0)
                {
                    MessageBox.Show("No smart card readers are connected to this system!");
                    return null;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("No smart card readers are connected to this system!");
                return null;
            }

			// Try to connect to a smart card inside any of the readers that are connected
			// to this system. This could be simplified if we only have to deal with a
			// single kind of reader that is known in advance.

			CardExpress	aCard = new CardExpress(m_aCardResourceManager);

			try
			{
				for (;;)
				{
					bool fConnected = false;

					foreach (string sReaderName in vsReaderNames)
					{
						fConnected = aCard.Connect(sReaderName, SCardAccessMode.Exclusive,
							SCardProtocolIdentifiers.T0 | SCardProtocolIdentifiers.T1);
						if (fConnected)
						{
							break;
						}
					}
					if (fConnected)
					{
						break;
					}

					// No card was found. So we prompt the user to insert one.

					DialogResult nOK = m_aCardForm.InsertCard(this,
						"No SIM card found",
						"Please insert a GSM SIM card into any of the smart card readers " +
						"that are listed below.",
						(string[])vsReaderNames.ToArray(typeof(string)));
					if (nOK == DialogResult.Cancel)
					{
						aCard.Dispose();
						return null;
					}
				}
			}
			catch
			{
				if (aCard != null)
				{
					aCard.Dispose();
					aCard = null;
				}

				MessageBox.Show("Error accessing GSM SIM card!");
				return null;
			}

            return new SIMAuthDaemon(aCard, LogMessage);
		}

        StringBuilder Builder = new StringBuilder();
        protected string DumpBytes(byte[] data)
        {
            Builder.Length = 0;

            foreach (byte value in data)
            {
                Builder.AppendFormat("{0:X02} ", value);
            }

            return Builder.ToString();
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="aSim"></param>

		private void _DisposeSim(SIMAuthDaemon aSim)
		{
            if (aSim == null)
            {
                return;
            }
			aSim.Card.Disconnect(SCardDisposition.UnpowerCard);
			aSim.Card.Dispose();
		}

    }
}
