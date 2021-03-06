using System;
using System.Collections;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using LibRXFFT.Libraries.GSM.Layer3;

namespace GSM_Analyzer
{
    public partial class FilterDialog : Form
    {
        private bool Initializing = true;
        private static bool AlreadyLoaded = false;

        public FilterDialog()
        {
            InitializeComponent();

            /* add the possbile mesage fields that cause to dump the message */
            lstExcept.Items.Add("IMEI");
            lstExcept.Items.Add("IMEISV");
            lstExcept.Items.Add("IMSI");
            lstExcept.Items.Add("TMSI/P-TMSI");
            lstExcept.Items.Add("Emergency call");


            /* add all known messages */
            foreach (L3MessageInfo info in L3Handler.L3MessagesRadio.Map.Values)
                lstFiltered.Items.Add(info);

            /* pre-select the already skipped messages */
            lock (L3Handler.SkipMessages)
            {
                lstFiltered.SelectedItems.Clear();
                foreach (string reference in L3Handler.SkipMessages.Keys)
                {
                    L3MessageInfo selectedItem = L3Handler.L3MessagesRadio.Get(reference);
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

            chkExcept.Checked = L3Handler.ExceptFieldsEnabled;
            lstExcept.Enabled = L3Handler.ExceptFieldsEnabled;

            Initializing = false;

            if (!AlreadyLoaded)
            {
                try
                {
                    LoadXml("default.flt.xml");
                }
                catch (Exception)
                {
                }
                AlreadyLoaded = true;
            }
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
                SaveXml(dlg.FileName);
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "XML Filter files (*.flt.xml)|*.flt.xml|All files (*.*)|*.*";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                LoadXml(dlg.FileName);
            }
        }

        private void LoadXml(string fileName)
        {
            XmlSerializer ser = new XmlSerializer(typeof(FilterSettings));
            FileStream stream = new FileStream(fileName, FileMode.Open);
            try
            {
                FilterSettings container = (FilterSettings)ser.Deserialize(stream);

                lstFiltered.SelectedItems.Clear();
                foreach (string reference in container.FilteredMessages)
                {
                    L3MessageInfo selectedItem = L3Handler.L3MessagesRadio.Get(reference);
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

        private void SaveXml(string fileName)
        {
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

        private void chkExcept_CheckedChanged(object sender, EventArgs e)
        {
            L3Handler.ExceptFieldsEnabled = chkExcept.Checked;
            lstExcept.Enabled = L3Handler.ExceptFieldsEnabled;
        }

    }
}
