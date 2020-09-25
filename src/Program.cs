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
            AppCore.Core = new AppCore();
            SubmissionsDB.DB = new SubmissionsDB();
            GlobalSettings.GlobalSettingsInit();

            var args = Environment.GetCommandLineArgs();
            if (args.Count() < 2 || args[1] != "-b")
            {
                if (args.Count() >= 2)
                {
                    Console.WriteLine("Note: found some invalid command line arguments");
                    Console.WriteLine("For CLI usage notes run this app with -b -help");
                }
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                WebBrowserEmulationSet();
                Utils.EmbeddedIeUtils.SetKnownUserAgent();
                Application.Run(new authForm());
            }
            else
            {
                bool AuthRes = AppCore.Core.Init().Result;
                if (!AuthRes)
                {
                    Console.WriteLine("Not authorized! Log in at least once using GUI first.");
                    Console.WriteLine("Alternatively, provide the 'cookie' and 'user-agent' header values");
                    Console.WriteLine("via FURDOWN_COOKIES and FURDOWN_USERAGENT environment variables.");
                    return;
                }
                CommandLineInterface.Execute(args).Wait();
            }
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
					if (rk == null) return;
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
