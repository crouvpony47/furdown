using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace furdown
{
    public partial class taskForm : Form
    {
        private Form closeOnClosingThis = null;

        public taskForm(Form formToClose = null)
        {
            closeOnClosingThis = formToClose;
            InitializeComponent();
        }

        private void applyNSaveBtn_Click(object sender, EventArgs e)
        {
            try
            {
                Directory.CreateDirectory(systemPathBox.Text);
                Directory.CreateDirectory(downloadPathBox.Text);
            }
            catch (Exception E)
            {
                MessageBox.Show("Incorrect parameters:" + Environment.NewLine + E.Message);
                return;
            }
            GlobalSettings.Settings.downloadPath = downloadPathBox.Text;
            GlobalSettings.Settings.systemPath = systemPathBox.Text;
            GlobalSettings.Settings.filenameTemplate = filenameTemplateBox.Text;
            GlobalSettings.Settings.descrFilenameTemplate = descrFilenameBox.Text;
            mainTabControl.SelectedTab = tasksTab;
        }

        private void taskForm_Shown(object sender, EventArgs e)
        {
            downloadPathBox.Text = GlobalSettings.Settings.downloadPath;
            systemPathBox.Text = GlobalSettings.Settings.systemPath;
            filenameTemplateBox.Text = GlobalSettings.Settings.filenameTemplate;
            descrFilenameBox.Text = GlobalSettings.Settings.descrFilenameTemplate;
        }

        private async void galleryDownloadBtn_Click(object sender, EventArgs e)
        {
            Hide();
            List<string> subs = await AppCore.Core.CreateSubmissionsListFromGallery(galleryUrlBox.Text);
            var pr = await AppCore.Core.ProcessSubmissionsList(subs, galleryDescrCheckBox.Checked);
            string msg = "Downloaded {0} files.";
            if (pr.failedToDownload.Count > 0 || pr.failedToDownload.Count > 0)
                msg += " However, some files were not downloaded, those submission IDs are stored in get_sub_page_failed.log and download_failed.log";
            Show();
            MessageBox.Show(msg.Replace("{0}",pr.processedPerfectly.ToString()));
        }

        private void galleryUrlBox_Leave(object sender, EventArgs e)
        {
            if (galleryUrlBox.Text.Last() == '/')
                galleryUrlBox.Text = galleryUrlBox.Text.Substring(0, galleryUrlBox.Text.Length - 1);
        }

        private void taskForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            closeOnClosingThis.Close();
        }

        private void submUrlsTextBox_Leave(object sender, EventArgs e)
        {
            submUrlsTextBox.Text = submUrlsTextBox.Text.ToLower();
            string[] toremove = {
                "https://", "http://", "furaffinity.net/view/", "furaffinity.net/full/", "/", "www."
            };
            foreach(string r in toremove)
                submUrlsTextBox.Text = submUrlsTextBox.Text.Replace(r, "");
            if (!System.Text.RegularExpressions.Regex.IsMatch(submUrlsTextBox.Text, @"^[0-9\r\n]+$")
                && submUrlsTextBox.Text.CompareTo("") != 0)
            {
                submUrlsTextBox.Text = "";
                MessageBox.Show("Only IDs or URLs, one per line, are allowed.");
            }
        }

        private void submUrlsLoadPrvBtn_Click(object sender, EventArgs e)
        {
            string log = Path.Combine(GlobalSettings.Settings.systemPath, "latest_subs.log");
            if (File.Exists(log))
            {
                submUrlsTextBox.Lines = File.ReadAllLines(log);
            }
            else
            {
                MessageBox.Show("\"latest_subs.log\" does not seem to exist.");
            }
            submUrlsTextBox.Focus();
        }

        private void submUrlsLoadFileBtn_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.Cancel)
                return;
            string file = openFileDialog.FileName;
            try
            {
                submUrlsTextBox.Lines = File.ReadAllLines(file);
            }
            catch(Exception E)
            {
                MessageBox.Show("Error: " + Environment.NewLine + E.Message);
            }
            submUrlsTextBox.Focus();
        }

        private async void submUrlsDownloadBtn_Click(object sender, EventArgs e)
        {
            Hide();
            List<string> subs = new List<string>(submUrlsTextBox.Lines);
            var pr = await AppCore.Core.ProcessSubmissionsList(subs, submUrlsDescrCheckBox.Checked);
            string msg = "Downloaded {0} files.";
            if (pr.failedToDownload.Count > 0 || pr.failedToDownload.Count > 0)
                msg += " However, some files were not downloaded, those submission IDs are stored in get_sub_page_failed.log and download_failed.log";
            Show();
            MessageBox.Show(msg.Replace("{0}", pr.processedPerfectly.ToString()));
        }

        private void downloadPathBrowse_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.SelectedPath = downloadPathBox.Text;
            if (folderBrowserDialog.ShowDialog() == DialogResult.Cancel)
                return;
            downloadPathBox.Text = folderBrowserDialog.SelectedPath;
        }

        private void systemPathBrowse_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.SelectedPath = systemPathBox.Text;
            if (folderBrowserDialog.ShowDialog() == DialogResult.Cancel)
                return;
            systemPathBox.Text = folderBrowserDialog.SelectedPath;
        }
    }
}
