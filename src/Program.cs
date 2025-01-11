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
                // GUI mode
                if (args.Count() >= 2)
                {
                    Console.WriteLine("Note: found some invalid command line arguments");
                    Console.WriteLine("For CLI usage notes run this app with -b -help");
                }
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new authForm());
            }
            else
            {
                // batch mode
                bool hasEnvCookies = (Environment.GetEnvironmentVariable("FURDOWN_COOKIES") != null);
                bool authRes = false;
                if (hasEnvCookies)
                {
                    authRes  = AppCore.Core.Init().Result;
                }
                else
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    var aForm = new authForm();
                    aForm.BackgroundMode = true;
                    Application.Run(aForm);
                    authRes = AppCore.Core.isInitialized;
                }
                if (!authRes)
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
    }
}
