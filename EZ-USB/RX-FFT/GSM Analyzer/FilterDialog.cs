using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using LibRXFFT.Libraries.GSM.Layer1;
using LibRXFFT.Libraries.GSM.Layer3;

namespace GSM_Analyzer
{
    public partial class FilterDialog : Form
    {
        private bool Initializing = true;

        public FilterDialog()
        {
            InitializeComponent();

            /* add the possbile mesage fields that cause to dump the message */
            lstExcept.Items.Add("IMEI");
            lstExcept.Items.Add("IMEISV");
            lstExcept.Items.Add("IMSI");
            lstExcept.Items.Add("TMSI/P-TMSI");


            /* add all known messages */
            foreach (L3MessageInfo info in L3Handler.L3Messages.Map.Values)
                lstFiltered.Items.Add(info);

            /* pre-select the already skipped messages */
            lock (L3Handler.SkipMessages)
            {
                lstFiltered.SelectedItems.Clear();
                foreach (string reference in L3Handler.SkipMessages.Keys)
                {
                    L3MessageInfo selectedItem = L3Handler.L3Messages.Get(reference);
                    if (selectedItem != null)
                        lstFiltered.SelectedItems.Add(selectedItem);
                }
            }

            /* the same for the exception fields */
            lock (L3Handler.ExceptFields)
            {
                lstExcept.SelectedItems.Clear();
                foreach (string field in L3Handler.ExceptFields.Keys)
                    lstExcept.SelectedItems.Add(field);
            }

            Initializing = false;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "XML Filter files (*.flt.xml)|*.flt.xml|All files (*.*)|*.*";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                string fileName = dlg.FileName;
                if (!fileName.EndsWith(".flt.xml"))
                {
                    if (fileName.Contains("."))
                        fileName = fileName.Split('.')[0] + ".flt.xml";
                    else
                        fileName += ".flt.xml";
                }

                XmlSerializer ser = new XmlSerializer(typeof(FilterSettings));
                StreamWriter writer = new StreamWriter(fileName, false);
                try
                {
                    FilterSettings container = new FilterSettings();

                    foreach (L3MessageInfo selectedItem in lstFiltered.SelectedItems)
                        container.FilteredMessages.Add(selectedItem.Reference);
                    foreach (string field in lstExcept.SelectedItems)
                        container.ExceptionFields.Add(field);

                    ser.Serialize(writer, container);
                }
                finally
                {
                    writer.Close();
                }
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "XML Filter files (*.flt.xml)|*.flt.xml|All files (*.*)|*.*";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                XmlSerializer ser = new XmlSerializer(typeof(FilterSettings));
                FileStream stream = new FileStream(dlg.FileName, FileMode.Open);
                try
                {
                    FilterSettings container = (FilterSettings)ser.Deserialize(stream);

                    lstFiltered.SelectedItems.Clear();
                    foreach (string reference in container.FilteredMessages)
                    {
                        L3MessageInfo selectedItem = L3Handler.L3Messages.Get(reference);
                        for (int pos = 0; pos < lstFiltered.Items.Count; pos++)
                        {
                            if (lstFiltered.Items[pos].ToString() == selectedItem.ToString())
                                lstFiltered.SelectedItems.Add(lstFiltered.Items[pos]);
                        }
                    }

                    lstExcept.SelectedItems.Clear();
                    foreach (string field in container.ExceptionFields)
                    {
                        for (int pos = 0; pos < lstExcept.Items.Count; pos++)
                        {
                            if (lstExcept.Items[pos].ToString() == field)
                                lstExcept.SelectedItems.Add(lstExcept.Items[pos]);
                        }
                    }
                }
                finally
                {
                    stream.Close();
                }
            }
        }

        private void lstFiltered_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ( Initializing)
                return;

            L3Handler.SkipMessages.Clear();

            foreach (L3MessageInfo selectedItem in lstFiltered.SelectedItems)
            {
                L3Handler.SkipMessages.Add(selectedItem.Reference, true);
            }
        }

        private void lstExcept_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Initializing)
                return;

            L3Handler.ExceptFields.Clear();

            foreach (string field in lstExcept.SelectedItems)
            {
                L3Handler.ExceptFields.Add(field, true);
            }
        }

        public class FilterSettings
        {
            public ArrayList FilteredMessages = new ArrayList();
            public ArrayList ExceptionFields = new ArrayList();
        }

    }
}
