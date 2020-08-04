using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        /// <summary>
        /// Save new settings and apply them
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            GlobalSettings.Settings.downloadOnlyOnce = neverDownloadTwiceCheckBox.Checked;
            GlobalSettings.GlobalSettingsSave();
            mainTabControl.SelectedTab = tasksTab;
        }

        /// <summary>
        /// Load settings into GUI elements.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void taskForm_Shown(object sender, EventArgs e)
        {
            downloadPathBox.Text = GlobalSettings.Settings.downloadPath;
            systemPathBox.Text = GlobalSettings.Settings.systemPath;
            filenameTemplateBox.Text = GlobalSettings.Settings.filenameTemplate;
            descrFilenameBox.Text = GlobalSettings.Settings.descrFilenameTemplate;
            neverDownloadTwiceCheckBox.Checked = GlobalSettings.Settings.downloadOnlyOnce;
        }

        private async void galleryDownloadBtn_Click(object sender, EventArgs e)
        {
            Hide();
            string link = galleryUrlBox.Text;
            try
            {
                var pr = await AppCore.Core.ProcessGenericUrl(link, galleryDescrCheckBox.Checked, galleryUpdateCheckBox.Checked);
                string msg = "Downloaded {0} files.";
                if (pr.failedToDownload.Count > 0 || pr.failedToGetPage.Count > 0)
                    msg += " However, some files were not downloaded, those submission IDs are stored in get_sub_page_failed.log and download_failed.log";
                Show();
                MessageBox.Show(msg.Replace("{0}", pr.processedPerfectly.ToString()));
            }
            catch (Exception ex)
            {
                Show();
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void galleryUrlBox_Leave(object sender, EventArgs e)
        {
            if (galleryUrlBox.Text != "" && galleryUrlBox.Text.Last() == '/')
                galleryUrlBox.Text = galleryUrlBox.Text.Substring(0, galleryUrlBox.Text.Length - 1);
        }

        private void taskForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            closeOnClosingThis.Close();
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
            {
                return;
            }
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
            var pr = await AppCore.Core.ProcessSubmissionsList(subs, submUrlsDescrCheckBox.Checked, submUrlsUpdateCheckBox.Checked);
            string msg = "Downloaded {0} files.";
            if (pr.failedToDownload.Count > 0 || pr.failedToGetPage.Count > 0)
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

        #region submissions lists validation
        /// <summary>
        /// Extracts submissions IDs from text containing URLs and newline characters.
        /// Returns false if any unexpected characters are found.
        /// </summary>
        private static bool submUrlsListValidate(ref string text)
        {
            text = text.ToLower();
            string[] toremove = {
                "https://", "http://", "furaffinity.net/view/", "furaffinity.net/full/", "/", "www."
            };
            foreach (string r in toremove)
                text = text.Replace(r, "");
            var lines = text.Split("\r\n".ToCharArray());
            foreach (var line in lines)
            {
                if (!string.IsNullOrEmpty(line)
                    && !Regex.IsMatch(line, @"^[0-9]+$")
                    && !Regex.IsMatch(line, @"^[0-9]+#[0-9]+$"))
                {
                    return false;
                }
            }
            return true;
        }

        private void submUrlsTextBox_Leave(object sender, EventArgs e)
        {
            string s = submUrlsTextBox.Text;
            if (!submUrlsListValidate(ref s))
            {
                submUrlsTextBox.Text = "";
                MessageBox.Show("Only IDs or URLs, one per line, are allowed.");
            }
            else
            {
                submUrlsTextBox.Text = s;
            }
        }

        private void dbSubmTextBox_Leave(object sender, EventArgs e)
        {
            string s = dbSubmTextBox.Text;
            if (!submUrlsListValidate(ref s))
            {
                dbSubmTextBox.Text = "";
                MessageBox.Show("Only IDs or URLs, one per line, are allowed.");
            }
            else
            {
                dbSubmTextBox.Text = s;
            }
        }
        #endregion

        private void clearDbBtn_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to clear the database?", "<!>", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                SubmissionsDB.DB.Clear();
                MessageBox.Show("Done.");
            }
        }

        private void addIdsToDbBtn_Click(object sender, EventArgs e)
        {
            int counter = 0;
            string[] lines = dbSubmTextBox.Lines;
            dbSubmTextBox.Text = "";
            for (int i = 0; i < lines.Count(); i++)
            {
                try
                {
                    if (SubmissionsDB.DB.AddSubmission(uint.Parse(lines[i])))
                        counter++;
                    else
                        dbSubmTextBox.Text += lines[i] + Environment.NewLine;
                }
                catch { } // not an integer - do not care
            }
            SubmissionsDB.Save();
            MessageBox.Show(counter.ToString() + " submissions have been added.");
        }

        private void removeIdsFromDb_Click(object sender, EventArgs e)
        {
            int counter = 0;
            string[] lines = dbSubmTextBox.Lines;
            dbSubmTextBox.Text = "";
            for (int i = 0; i < lines.Count(); i++)
            {
                try
                {
                    if (SubmissionsDB.DB.RemoveSubmission(uint.Parse(lines[i])))
                        counter++;
                    else
                        dbSubmTextBox.Text += lines[i] + Environment.NewLine;
                }
                catch { } // not an integer - do not care
            }
            SubmissionsDB.Save();
            MessageBox.Show(counter.ToString() + " submissions have been removed.");
        }

        private void useLinksListBtn_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }
            galleryUrlBox.Text = "$" + openFileDialog.FileName;
            galleryDownloadBtn.Focus();
        }
    }
}
