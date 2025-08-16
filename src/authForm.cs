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
            try
            {
                edgeWebView.CoreWebView2.AddWebResourceRequestedFilter(
                      "https://www.furaffinity.net/*",
                      CoreWebView2WebResourceContext.Document,
                      CoreWebView2WebResourceRequestSourceKinds.All);
            }
            catch (Exception)
            {
                // fallback to an older (deprecated) filter interface
                // for legacy WebView versions (i.e. v.109 on Win 7)
                edgeWebView.CoreWebView2.AddWebResourceRequestedFilter(
                      "https://www.furaffinity.net/*",
                      CoreWebView2WebResourceContext.Document);
            }
            edgeWebView.CoreWebView2.WebResourceRequested += async delegate (
               object _sender, CoreWebView2WebResourceRequestedEventArgs args)
            {
                var uri = new Uri(args.Request.Uri);
                if (uri.AbsolutePath.ToString() == "/" || uri.AbsolutePath.ToString().StartsWith("/user")
                    || (uri.AbsolutePath.ToString().StartsWith("/login") && uri.Query.ToString().Contains("__cf")))
                {
                    CoreWebView2HttpRequestHeaders requestHeaders = args.Request.Headers;
                    var cookies = await edgeWebView.CoreWebView2.CookieManager.GetCookiesAsync("https://www.furaffinity.net");
                    if (cookies.Count > 0)
                    {
                        var cookieString = new StringBuilder();
                        foreach (var cookie in cookies)
                        {
                            cookieString.Append(cookie.Name + "=" + cookie.Value + "; ");
                        }
                        cookieString.Remove(cookieString.Length - 2, 2);
                        // Console.WriteLine("Cookies: " + cookieString.ToString());
                        Console.WriteLine("Extracted " + cookies.Count.ToString() + " cookies.");
                        CookiesStorage.SetCookieString(cookieString.ToString());
                    }
                    if (requestHeaders.Contains("User-Agent"))
                    {
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
            if (BackgroundMode)
            {
                Close();
                return;
            }
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
                if (BackgroundMode)
                {
                    Close();
                    return;
                }

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
            if (BackgroundMode)
            {
                WindowState = FormWindowState.Minimized;
            }
        }

        public bool BackgroundMode = false;

        delegate Task CookieValidationCallback();
        private CookieValidationCallback onShouldValidateCookies = null;
    }
}
