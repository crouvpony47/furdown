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
            taskForm tf = new taskForm(this);
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
                Console.WriteLine("Does not seem to be authorized (or need to pass CF validation)...");
                authWebBrowser.Navigate("https://www.furaffinity.net/login/");
            }
            else
            {
                OnAuthSuccessful();
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
    }
}
