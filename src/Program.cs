using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace furdown
{
    static class Program
    {
        /// <summary>
        /// App entry point.
        /// </summary>
        [STAThread]
        static void Main()
        {
            GlobalSettings.GlobalSettingsInit();
            AppCore.Core = new AppCore();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            WebBrowserEmulationSet();
            Application.Run(new authForm());
            AppCore.Core.OnAppTerminate();
        }

        /// <summary>
        /// Sets WebBrowser components to IE11 mode, rather than default IE7,
        /// which is really not suitable for anything.
        /// </summary>
        static void WebBrowserEmulationSet()
        {
            try
            {
                using (
                    var rk = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    @"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", true)
                )
                {
                    string appname = System.IO.Path.GetFileName(Application.ExecutablePath);
                    dynamic value = rk.GetValue(appname);
                    if (value == null)
                        rk.SetValue(appname, (uint)11001, Microsoft.Win32.RegistryValueKind.DWord);
                }
            }
            catch (Exception E)
            {
                MessageBox.Show("Something went wrong:" + Environment.NewLine + E.Message);
            }
        }
    }
}
