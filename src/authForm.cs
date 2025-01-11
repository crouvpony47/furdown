using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.WebRequestMethods;

namespace furdown
{
    public partial class authForm : Form
    {
        public authForm()
        {
            InitializeComponent();
            edgeWebView.Visible = false;
        }

        // Sets up WebView and its listener, and performs an initial cookie extraction attempt.
        // Returns true if login validated successfully.
        public async Task<bool> Start()
        {
            // initialize WebView2 fully
            await edgeWebView.EnsureCoreWebView2Async(null);
            // initialize request interception
            edgeWebView.CoreWebView2.AddWebResourceRequestedFilter(
                  "https://www.furaffinity.net/*",
                  CoreWebView2WebResourceContext.Document,
                  CoreWebView2WebResourceRequestSourceKinds.All);
            edgeWebView.CoreWebView2.WebResourceRequested += async delegate (
               object _sender, CoreWebView2WebResourceRequestedEventArgs args)
            {
                var uri = new Uri(args.Request.Uri);
                if (uri.AbsolutePath.ToString() == "/" || uri.AbsolutePath.ToString().StartsWith("/user"))
                {
                    CoreWebView2HttpRequestHeaders requestHeaders = args.Request.Headers;
                    if (requestHeaders.Contains("Cookie"))
                    {
                        CookiesStorage.SetCookieString(requestHeaders.GetHeader("Cookie"));
                        CookiesStorage.SetAssociatedUserAgent(requestHeaders.GetHeader("User-Agent"));
                    }
                    if (onShouldValidateCookies != null)
                    {
                        await onShouldValidateCookies();
                    }
                }
            };

            var tcs = new TaskCompletionSource<bool>();
            onShouldValidateCookies = async delegate ()
            {
                bool authRes = await AppCore.Core.Init();
                tcs.TrySetResult(authRes);
            };

            // navigate to the front page, this will also trigger cookie validation
            edgeWebView.Source = new Uri("https://www.furaffinity.net/");

            var result = await tcs.Task;
            onShouldValidateCookies = null;
            return result;
        }

        private void OnAuthSuccessful()
        {
            Hide();
            taskForm tf = new taskForm(this, AppCore.Core.defaultUserId);
            tf.Show();
        }

        private async void WebViewLoginFlow()
        {
            try
            {
                Console.WriteLine("WebView2 version: " + CoreWebView2Environment.GetAvailableBrowserVersionString());
            }
            catch (WebView2RuntimeNotFoundException)
            {
                const string wv2downloadUrl = "https://go.microsoft.com/fwlink/p/?LinkId=2124703";
                var dlgResult = MessageBox.Show("WebView2 runtime is required, but was not found!\n"
                    + "Open download page now?", "Missing Component", MessageBoxButtons.YesNo);
                if (dlgResult == DialogResult.Yes)
                {
                    Utils.OpenUrl(wv2downloadUrl);
                }
                Close();
            }

            var authResult = await Start();

            if (authResult) // stored cookies were sufficient
            {
                OnAuthSuccessful();
            }
            else
            {
                Console.WriteLine("Does not seem to be authorized (or need to pass CF validation)...");

                // set up a new login callback
                onShouldValidateCookies = async delegate ()
                {
                    bool authRes = await AppCore.Core.Init();
                    if (authRes)
                    {
                        OnAuthSuccessful();
                    }
                    else
                    {
                        Console.WriteLine("Unsuccessfull login attempt!");
                    }
                };

                edgeWebView.Source = new Uri("https://www.furaffinity.net/login/");
                loadingLabel.SendToBack();
                loadingLabel.Hide();
                edgeWebView.Visible = true;
            }
        }

        private async void authForm_Shown(object sender, EventArgs e)
        {
            string envCookies = Environment.GetEnvironmentVariable("FURDOWN_COOKIES");
            if (envCookies != null)
            {
                bool authRes = await AppCore.Core.Init();
                if (authRes)
                {
                    OnAuthSuccessful();
                }
                else
                {
                    Console.WriteLine("FURDOWN_COOKIES found, but the provided cookies and User-Agent were not sufficient to log in!");
                    Close();
                }
            }
            else
            {
                WebViewLoginFlow();
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
                    Utils.OpenUrl(urlToOpen);
                }
            }
        }

        private void authForm_Load(object sender, EventArgs e)
        {
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }

        delegate Task CookieValidationCallback();
        private CookieValidationCallback onShouldValidateCookies = null;
    }
}
