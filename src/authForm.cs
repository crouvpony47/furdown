using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace furdown
{
    public partial class authForm : Form
    {
        public authForm()
        {
            InitializeComponent();
        }

        void OnAuthSuccessful()
        {
            Hide();
            taskForm tf = new taskForm(this, AppCore.Core.defaultUserId);
            tf.Show();
        }

        private async void authForm_Shown(object sender, EventArgs e)
        {
            // set 'splash' text to appear while the cookies are checked at startup
            authWebBrowser.Navigate("about:blank");
            if (authWebBrowser.Document != null)
                authWebBrowser.Document.Write(string.Empty);
            authWebBrowser.DocumentText = "<body>Validating cookies...</body>";
            // validate cookies already present in system
            bool AuthRes = await AppCore.Core.Init();
            if (!AuthRes)
            {
                if (Environment.GetEnvironmentVariable("FURDOWN_COOKIES") == null)
                {
                    Console.WriteLine("Does not seem to be authorized (or need to pass CF validation)...");
                    authWebBrowser.Navigate("https://www.furaffinity.net/login/");
                }
                else
                {
                    MessageBox.Show("FURDOWN_COOKIES environment variable is set, but the cookies provided " +
                                    "could not be used to authenticate the user. Please set a valid value " +
                                    "or unset FURDOWN_COOKIES to use the default login mechanism.");
                    Close();
                }
            }
            else
            {
                OnAuthSuccessful();
            }

            // check for updates
            bool hasUpdates = await UpdatesChecker.CheckRemoteVersion();
            const string urlToOpen = "https://github.com/crouvpony47/furdown/releases";
            if (hasUpdates && Form.ActiveForm != null && Form.ActiveForm.Visible)
            {
                var dlgResult = MessageBox.Show("A newer version of furdown is available, would you like to download it?",
                                                "Update Available",
                                                MessageBoxButtons.YesNo);
                if (dlgResult == DialogResult.Yes)
                {
                    try
                    {
                        System.Diagnostics.Process.Start(urlToOpen);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Could not open URL in the default browser:\n" + urlToOpen);
                    }
                }
            }
        }

        private async void authWebBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            //Console.WriteLine(e.Url.AbsolutePath.ToString());
            
            // redirected to the title page
            if (e.Url.AbsolutePath.ToString().CompareTo("/") == 0)
            {
                bool AuthRes = await AppCore.Core.Init();
                if (AuthRes)
                {
                    OnAuthSuccessful();
                }
                else
                {
                    Console.WriteLine(string.Format("Unsuccessfull login attempt!"));
                }
            }
        }

        private void authForm_Load(object sender, EventArgs e)
        {
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }
    }
}
