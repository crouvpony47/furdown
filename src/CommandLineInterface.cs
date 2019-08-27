using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace furdown
{
    class CommandLineInterface
    {
        public static async Task Execute(string[] args)
        {
            bool needDescriptions = false;
            string url = "";
            for (int i = 1; i < args.Count(); i++)
            {
                switch (args[i])
                {
                    case "-b":
                        continue;
                    // set gallery url
                    case "-g":
                        if (++i < args.Count())
                        {
                            url = args[i];
                            continue;
                        }
                        else goto badArgs;
                    // set file path
                    case "-f":
                        if (++i < args.Count())
                        {
                            url = "$"+args[i];
                            continue;
                        }
                        else goto badArgs;
                    // enable description saving
                    case "-d":
                        needDescriptions = true;
                        continue;
                    // custom output path
                    case "-o":
                        if (++i < args.Count())
                        {
                            GlobalSettings.Settings.downloadPath = args[i];
                            continue;
                        }
                        else goto badArgs;
                    // custom subscriptions name pattern
                    case "-st":
                        if (++i < args.Count())
                        {
                            GlobalSettings.Settings.filenameTemplate = args[i];
                            continue;
                        }
                        else goto badArgs;
                    // custom descriptions name pattern
                    case "-dt":
                        if (++i < args.Count())
                        {
                            GlobalSettings.Settings.descrFilenameTemplate = args[i];
                            continue;
                        }
                        else goto badArgs;
                    // download-once option
                    case "-dbi":
                    case "-dbignore":
                        GlobalSettings.Settings.downloadOnlyOnce = true;
						continue;
                    case "-dbf":
                    case "-dbforce":
                        GlobalSettings.Settings.downloadOnlyOnce = false;
						continue;
                    // anything else: print basic usage notes
                    case "-h":
                    case "-help":
                    case "-?":
                    default:
                        Console.WriteLine("\nUsage:\n"
                                        + "-b       Enter CLI mode. Must be first.\n"
                                        + "-g URL   Download gallery with url URL.\n"
                                        + "         Works for scraps and folders as well.\n"
                                        + "-f FILE  Load url list from FILE.\n"
                                        + "         Same as -g $FILE\n"
                                        + "-o DIR   Override output directory with DIR.\n"
                                        + "-st TMP  Override submissions naming template with TMP\n"
                                        + "-dt TMP  Override descriptions naming template with TMP\n"
                                        + "-d       Enable downloading HTML descriptions.\n"
                                        + "-dbi     Prohibits re-downloading files already marked\n"
                                        + "         as downloaded in the internal database\n"
                                        + "-dbf     Allows re-downloading files already marked\n"
                                        + "         as downloaded in the internal database"
                        );
                        return;
                }
                badArgs:
                    Console.WriteLine("Missing argument for " + args[i - 1]);
                    return;
            }

            if (url == "")
            {
                Console.WriteLine("No gallery url has been provided.");
                Console.WriteLine("For usage notes run this app with -b -help");
                return;
            }

            try
            {
                var pr = await AppCore.Core.ProcessGenericUrl(url, needDescriptions);
                string msg = "Downloaded {0} files.";
                if (pr.failedToDownload.Count > 0 || pr.failedToGetPage.Count > 0)
                    msg += " However, some files were not downloaded, those submission IDs are stored in get_sub_page_failed.log and download_failed.log";
                Console.WriteLine(msg.Replace("{0}", pr.processedPerfectly.ToString()));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
        }
    }
}
