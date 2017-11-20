using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;

namespace furdown
{
    public class AppCore
    {
        public static AppCore Core;

        #region console management
        [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        public static extern bool FreeConsole();
        #endregion

        #region HTTP-related members
        private HttpClientHandler httph = null;
        private HttpClient http = null;

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool InternetGetCookieEx(string pchURL, string pchCookieName, StringBuilder pchCookieData, ref uint pcchCookieData, int dwFlags, IntPtr lpReserved);
        private const int INTERNET_COOKIE_HTTPONLY = 0x00002000;
        #endregion

        /// <summary>
        /// Gets all, including http-only, cookies from WebBrowser component
        /// </summary>
        /// <param name="uri">URI which is used to get the cookies for</param>
        /// <returns></returns>
        private static string GetGlobalCookies(string uri)
        {
            uint datasize = 1024;
            StringBuilder cookieData = new StringBuilder((int)datasize);
            if (InternetGetCookieEx(
                uri, null, cookieData, ref datasize, INTERNET_COOKIE_HTTPONLY, IntPtr.Zero
            ) && cookieData.Length > 0)
            {
                return cookieData.ToString();
            }
            else
            {
                return null;
            }
        }
        
        public AppCore()
        {
            // get the console windows
            AllocConsole();
            // welcome thing
            Console.WriteLine(@"furdown " + Assembly.GetEntryAssembly().GetName().Version);
            Console.WriteLine(@"<crouvpony47.itch.io> <github.com/crouvpony47>");
            // initialize http client
            httph = new HttpClientHandler();
            httph.UseCookies = false; // disable internal cookies handling to help with import
            http = new HttpClient(httph);
            http.DefaultRequestHeaders.Clear();
        }

        /// <summary>
        /// Free resources on app termination
        /// </summary>
        public void OnAppTerminate()
        {
            FreeConsole();
        }

        /// <summary>
        /// Checks whether valid cookies are present in system or not
        /// </summary>
        /// <returns>True if valid cookies are found</returns>
        public async Task<bool> Init()
        {
            Console.WriteLine("Checking authorization...");
            string cookies = GetGlobalCookies("https://www.furaffinity.net/");
            if (cookies == null) return false;
            Console.WriteLine("Found cookies: "+cookies);
            try
            {
                http.DefaultRequestHeaders.Clear();
                http.DefaultRequestHeaders.Add("Cookie", cookies);
                string cpage = await http.GetStringAsync("https://www.furaffinity.net/");
                // authorized
                if (cpage.Contains("\"logout-link\""))
                {
                    // and not using classic style
                    if (!cpage.Contains("/themes/classic/"))
                    {
                        Console.WriteLine("Auth check completed successfully.");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("Error: classic theme detected.");
                        System.Windows.Forms.MessageBox.Show(
                            "It seems you're using the classic style." + Environment.NewLine
                            + "Please switch over to the new one and return to the title page once again."
                            );
                        return false;
                    }
                }
                // not authorized
                else
                {
                    Console.WriteLine("Not authorized.");
                    return false;
                }
            }
            // any unaccounted errors
            catch
            {
                return false;
            };
        }

        public async Task<List<string>> CreateSubmissionsListFromGallery(string gallery)
        {
            Console.WriteLine("Building submissions list for " + gallery);
            List<string> lst = new List<string>();
            int page = 0;
            while (true)
            {
                page++;
                string pageUrl = gallery + "/" + page.ToString();
                string key = "figure id=\"sid-";
                string endkey = "\"";
                // get page listing submissions
                int attempts = 3;
                string cpage = "";
                beforeawait:
                try
                {
                    Console.WriteLine("GET await: " + pageUrl);
                    cpage = await http.GetStringAsync(pageUrl);
                }
                catch (Exception E)
                {
                    Console.WriteLine("GET error (" + pageUrl + "): " + E.Message);
                    attempts--;
                    System.Threading.Thread.Sleep(2000);
                    if (attempts > 0)
                        goto beforeawait;
                    else
                        return null;
                }
                // find submissions in the page
                int counter = 0;
                while (cpage.Contains(key))
                {
                    counter++;
                    cpage = cpage.Substring(cpage.IndexOf(key, StringComparison.Ordinal) + key.Length);
                    lst.Add(cpage.Substring(0, cpage.IndexOf(endkey, StringComparison.Ordinal)));
                }
                if (counter == 0)
                {
                    Console.WriteLine("Reached an empty page. Total elements found: " + lst.Count.ToString());
                    break;
                }
            }
            try
            {
                Console.WriteLine("Submissions list saved to \"latest_subs.log\"");
                File.WriteAllLines(Path.Combine(GlobalSettings.Settings.systemPath, "latest_subs.log"), lst);
            }
            catch (Exception E)
            {
                Console.WriteLine("Failed to save submissions list: " + E.Message);
            }
            return lst;
        }

        public class ProcessingResults
        {
            public int processedPerfectly;
            public List<string> failedToGetPage;
            public List<string> failedToDownload;
            internal ProcessingResults()
            {
                processedPerfectly = 0;
                failedToGetPage = new List<string>();
                failedToDownload = new List<string>();
            }
        }

        public async Task<ProcessingResults> ProcessSubmissionsList(List<string> subs, bool needDescription)
        {
            Console.WriteLine("Processing submissions list...");
            ProcessingResults res = new ProcessingResults();
            // iterate over all the submissions in list
            for (int i = subs.Count - 1; i >= 0; i--)
            {
                string subId = subs[i];
                // don't care about empty strings
                if (subId == null || subId.CompareTo("") == 0) continue;
                // check if in DB already
                try
                {
                    if (SubmissionsDB.DB.Exists(uint.Parse(subId)) 
                        && GlobalSettings.Settings.downloadOnlyOnce) continue;
                }
                catch
                {
                    Console.WriteLine("Unexpected ");
                    continue;
                }
                string subUrl = "https://www.furaffinity.net/view/" + subId;
                // get submission page
                int attempts = 3;
                string cpage = "";
                beforeawait:
                try
                {
                    Console.WriteLine("GET await: " + subUrl);
                    cpage = await http.GetStringAsync(subUrl);
                }
                catch (Exception E)
                {
                    Console.WriteLine("GET error (" + subUrl + "): " + E.Message);
                    attempts--;
                    System.Threading.Thread.Sleep(2000);
                    if (attempts > 0)
                        goto beforeawait;
                    else
                    {
                        Console.WriteLine("Giving up on {0}", subId);
                        res.failedToGetPage.Add(subId);
                        continue;
                    }
                }
                // process submission page
                string downbtnkey = "<a href=\"//d.facdn.net/";
                string desckey = "<div class=\"submission-description-container";
                SubmissionProps sp = new SubmissionProps();
                sp.SUBMID = subId;
                int keypos = cpage.IndexOf(downbtnkey, StringComparison.Ordinal);
                if (keypos < 0)
                {
                    Console.WriteLine("[Warning] got page, but it doesn't contain any download links.");
                    res.failedToGetPage.Add(subId);
                    continue;
                }
                cpage = cpage.Substring(keypos);
                cpage = cpage.Substring(cpage.IndexOf("/", StringComparison.Ordinal));
                sp.URL = "https:"
                    + cpage.Substring(0, cpage.IndexOf("\"", StringComparison.Ordinal));
                if (needDescription) {
                    cpage = cpage.Substring(cpage.IndexOf(desckey, StringComparison.Ordinal));
                    string desckeyend = "</div>";
                    cpage = cpage.Substring(0,
                        cpage.IndexOf(desckeyend, cpage.IndexOf(desckeyend) + 1) + desckeyend.Length
                    );
                    cpage.Replace("href=\"/", "href=\"https://furaffinity.net/");
                    cpage.Replace("src=\"//", "src=\"https://");
                }
                sp.FILEFULL = sp.URL.Substring(sp.URL.LastIndexOf('/') + 1);
                // remove characters windows filename can't hold
                sp.FILEFULL = string.Concat(sp.FILEFULL.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
                sp.FILEID = sp.FILEFULL.Substring(0, sp.FILEFULL.IndexOf('.'));
                sp.ARTIST = sp.FILEFULL.Substring(sp.FILEFULL.IndexOf('.') + 1);
                sp.ARTIST = sp.ARTIST.Substring(0, sp.ARTIST.IndexOf('_'));
                sp.FILEPART = sp.FILEFULL.Substring(sp.FILEFULL.IndexOf('_') + 1);
                sp.EXT = (sp.FILEFULL + " ").Substring(sp.FILEFULL.LastIndexOf('.') + 1).TrimEnd();
                // apply template(s)
                string fname = GlobalSettings.Settings.filenameTemplate;
                string dfname = GlobalSettings.Settings.descrFilenameTemplate;
                foreach (FieldInfo fi
                    in sp.GetType().GetFields(
                    BindingFlags.Instance | BindingFlags.Public).ToArray()
                )
                {
                    if (fi.FieldType == typeof(string))
                    {
                        fname = fname.Replace("%" + fi.Name + "%", (string)fi.GetValue(sp));
                        dfname = dfname.Replace("%" + fi.Name + "%", (string)fi.GetValue(sp));
                    }
                }
                // make sure directories exist
                string fnamefull = Path.Combine(GlobalSettings.Settings.downloadPath, fname);
                string dfnamefull = Path.Combine(GlobalSettings.Settings.downloadPath, dfname);
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fnamefull));
                    Directory.CreateDirectory(Path.GetDirectoryName(dfnamefull));
                }
                catch
                {
                    Console.WriteLine("Failed to make sure target directories do exist.");
                    break;
                }
                // save description
                if (needDescription)
                {
                    try
                    {
                        File.WriteAllText(dfnamefull, cpage);
                        Console.WriteLine("description saved to filename:" + dfname);
                    }
                    catch (Exception E)
                    {
                        Console.WriteLine("Error saving description:" + E.Message);
                    }
                    
                }
                // download file
                Console.WriteLine("target filename: " + fname);
                if (File.Exists(fnamefull))
                {
                    SubmissionsDB.DB.AddSubmission(uint.Parse(subId));
                    Console.WriteLine("Already exists, continuing~");
                    continue;
                }
                int fattempts = 3;
                fbeforeawait:
                try
                {
                    Console.WriteLine("GET await: " + sp.URL);
                    using (
                        Stream contentStream = await (await http.GetAsync(sp.URL)).Content.ReadAsStreamAsync(),
                        stream = new FileStream(fnamefull, 
                            FileMode.Create, FileAccess.Write, FileShare.None, 1024*1024 /*Mb*/, true))
                    {
                        await contentStream.CopyToAsync(stream);
                        SubmissionsDB.DB.AddSubmission(uint.Parse(subId));
                    }
                }
                catch (Exception E)
                {
                    Console.WriteLine("GET error (file " + sp.FILEID + "): " + E.Message);
                    fattempts--;
                    System.Threading.Thread.Sleep(2000);
                    if (fattempts > 0)
                        goto fbeforeawait;
                    {
                        Console.WriteLine("Giving up on downloading {0}", subId);
                        res.failedToDownload.Add(subId);
                        continue;
                    }
                }
                res.processedPerfectly++;
            }
            // writing results
            try
            {
                if (res.failedToGetPage.Count > 0) 
                    File.WriteAllLines(Path.Combine(GlobalSettings.Settings.systemPath, "get_sub_page_failed.log"), res.failedToGetPage);
                if (res.failedToDownload.Count > 0)
                    File.WriteAllLines(Path.Combine(GlobalSettings.Settings.systemPath, "download_failed.log"), res.failedToGetPage);
            }
            catch (Exception E)
            {
                Console.WriteLine("Failed to save list of subs with issues: " + E.Message);
            }
            // save DB
            SubmissionsDB.Save();
            // return result, actually
            return res;
        }

    }
}
