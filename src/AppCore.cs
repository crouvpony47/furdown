using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Globalization;

namespace furdown
{
    public class AppCore
    {
        public static AppCore Core;

        #region console management
        #if !furdown_portable_core
        [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("kernel32.dll")]
        public static extern bool FreeConsole();
        
        IntPtr currentConsoleHandle;
        #else
		IntPtr currentConsoleHandle = (IntPtr)0;
        #endif
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

        /// <summary>
        /// Reads network stream to another stream, throwing excepton if no data was received in reasonable time
        /// </summary>
        /// <param name="from">Readable network stream to copy data from</param>
        /// <param name="to">Writable stream to copy data to</param>
        /// <param name="timeout">Timeout for receiving a single chunk of data in milliseconds</param>
        /// <returns></returns>
        async private Task ReadNetworkStream(Stream from, Stream to, int timeout)
        {
            const int bufferSize = 2048;
            int receivedBytes = -1;
            var buffer = new byte[bufferSize];
            while (receivedBytes != 0) // read until no more data available (or timeout exception is thrown)
            {
                using (var cancellationTokenSource = new System.Threading.CancellationTokenSource(timeout))
                {
                    using (cancellationTokenSource.Token.Register(() => from.Close()))
                    {
                        receivedBytes = await from.ReadAsync(buffer, 0, bufferSize, cancellationTokenSource.Token);
                    }
                }
                if (receivedBytes > 0)
                {
                    await to.WriteAsync(buffer, 0, receivedBytes);
                }
            }
        }

        public AppCore()
        {
            // show the console window, and get its handle
            #if !furdown_portable_core
            AllocConsole();
            currentConsoleHandle = GetConsoleWindow();
            #endif

            // welcome thing
            Console.WriteLine(@"furdown " + Assembly.GetEntryAssembly().GetName().Version);
            Console.WriteLine(@"<github.com/crouvpony47> <crouvpony47.itch.io>");
            // initialize http client
            httph = new HttpClientHandler();
            httph.AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate;
            httph.UseCookies = false; // disable internal cookies handling to help with import
            http = new HttpClient(httph);
            http.DefaultRequestHeaders.Clear();

            System.Net.ServicePointManager.SecurityProtocol = 
                System.Net.SecurityProtocolType.Tls12 | System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls;
        }

        /// <summary>
        /// Free resources on app termination
        /// </summary>
        public void OnAppTerminate()
        {
            #if !furdown_portable_core
            FreeConsole();
            #endif
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
            Console.WriteLine("Found some cookies.");
            // Console.WriteLine("Found cookies: "+cookies);
            try
            {
                http.DefaultRequestHeaders.Clear();
                // IE's and HttpClient's UAs MUST match for Cloudflare to recognize us
                http.DefaultRequestHeaders.Add("User-Agent", Utils.EmbeddedIeUtils.GetKnownUserAgentValue());
                // test:  "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko"
                //http.DefaultRequestHeaders.Add("Accept-Language", "en-US;q=0.8,en;q=0.5,ja;q=0.3"); // not strictly neccessary
                http.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
                http.DefaultRequestHeaders.Add("Accept", "*/*");
                http.DefaultRequestHeaders.Add("Cookie", cookies);
                string cpage = await http.GetStringAsync("https://www.furaffinity.net/");
                // authorized
                if (Regex.Match(cpage, "class=\"[^\"]*logout-link[^\"]*\"", RegexOptions.CultureInvariant).Success)
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
                    Console.WriteLine("Not authorized!");
                    return false;
                }
            }
            // any unaccounted errors
            catch (HttpRequestException e)
            {
                Console.WriteLine("Error: " + e.Message);
                Console.WriteLine("If it is caused by Clouflare validation, pass it and navigate to the FA main page.");
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            };
        }

        public async Task<List<string>> CreateSubmissionsListFromGallery(string gallery)
        {
            Console.WriteLine("Building submissions list for " + gallery);
            TaskbarProgress.SetState(currentConsoleHandle, TaskbarProgress.TaskbarStates.Indeterminate);
            List<string> lst = new List<string>();
            int page = 0;
            long favNextId = -1;
            while (true)
            {
                page++;
                string pageUrl = gallery + ((page == 1 ) ? "/" : ("/" + page.ToString()));
                if (favNextId >= 0)
                {
                    pageUrl = gallery + "/" + favNextId.ToString() + "/next";
                }
                string key = "figure id=\"sid-";
                string endkey = "\"";
                // get page listing submissions
                int attempts = 3;
                string cpage = "";
                beforeawait:
                try
                {
                    Console.WriteLine("Getting page: " + pageUrl);
                    cpage = await http.GetStringAsync(pageUrl);
                }
                catch (Exception E)
                {
                    Console.WriteLine("GET request error (" + pageUrl + "): " + E.Message);
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
                if (pageUrl.Contains("/favorites/"))
                {
                    var nextMatch = Regex.Match(cpage, @"href=""/favorites/.+?/(.+?)/next"">Next<", RegexOptions.CultureInvariant);
                    if (nextMatch.Success)
                    {
                        if (!long.TryParse(nextMatch.Groups[1].Value, out favNextId))
                        {
                            Console.WriteLine("Warning :: can't get the next page URL");
                            break;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Last page reached.");
                        break;
                    }
                }
            }
            TaskbarProgress.SetState(currentConsoleHandle, TaskbarProgress.TaskbarStates.NoProgress);
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
				if (string.IsNullOrEmpty(subId)) continue;
                Console.WriteLine("> Processing submission #" + subId);
                // check if in DB already
                try
                {
                    if (SubmissionsDB.DB.Exists(uint.Parse(subId))
                        && GlobalSettings.Settings.downloadOnlyOnce)
                    {
                        Console.WriteLine("Skipped (present in DB)");
                        continue;
                    }
                }
                catch
                {
                    Console.WriteLine("Unexpected error (DB presence check failed)!");
                    continue;
                }
                string subUrl = "https://www.furaffinity.net/view/" + subId;

                // get submission page
                int attempts = 3;
                string cpage = "";
                beforeawait:
                try
                {
                    Console.WriteLine("Getting page: " + subUrl);
                    cpage = await http.GetStringAsync(subUrl);
                }
                catch (Exception E)
                {
                    Console.WriteLine("GET request error (" + subUrl + "): " + E.Message);
                    attempts--;
                    System.Threading.Thread.Sleep(2000);
                    if (attempts > 0)
                        goto beforeawait;
                    else
                    {
                        Console.WriteLine("Giving up on #" + subId);
                        res.failedToGetPage.Add(subId);
                        continue;
                    }
                }

                // process submission page
                string downbtnkey = "<a href=\"//d.facdn.net/";
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

				// processing submission description; also extracts submission date and title
				{
					Utils.FillPropertiesFromDateTime(DateTime.Now, sp); // set Now as a fallback date
					sp.TITLE = "Unknown"; // fallback title

                    // title
                    const string key_title = @"<div class=""submission-title"">";
                    const string key_enddiv = "</div>";
                    cpage = cpage.Substring(cpage.IndexOf(key_title, StringComparison.Ordinal));
                    string sub_title_div = cpage.Substring(0,
                                                           cpage.IndexOf(key_enddiv, cpage.IndexOf(key_enddiv, StringComparison.Ordinal) + 1,
                                                                         StringComparison.Ordinal) + key_enddiv.Length);
                    var titleMatch = Regex.Match(sub_title_div, "<h2><p>(.+?)</p></h2>", RegexOptions.CultureInvariant);
                    if (titleMatch.Success)
                    {
                        sp.TITLE = Utils.StripIllegalFilenameChars(titleMatch.Groups[1].Value);
                        Console.WriteLine("Title: " + sp.TITLE);
                    }
                    else Console.WriteLine("Warning :: no submission title found!");

                    // replace relative date with the absolute one
                    string sub_date_strong = "";
                    var dateMatch = Regex.Match(cpage, "<strong.+?title=\"(.+?)\" class=\"popup_date\">(.+?)<.+?</strong>", RegexOptions.CultureInvariant);
					if (dateMatch.Success)
					{
						string dateMatchVal = dateMatch.Value;
						string dateTimeStr = dateMatch.Groups[1].Value; // fixed format date
                        string dateTimeStrFuzzy = dateMatch.Groups[2].Value;
                        
                        // depending on user settings, fuzzy and fixed times may be swapped
                        if (dateTimeStrFuzzy.Contains(" PM") || dateTimeStrFuzzy.Contains(" AM"))
                        {
                            var temporary = dateTimeStr;
                            dateTimeStr = dateTimeStrFuzzy;
                            dateTimeStrFuzzy = temporary;
                        }
                        
                        // replace relative date with a fixed format one
                            sub_date_strong = dateMatchVal.Replace(dateTimeStrFuzzy, dateTimeStr);

                        // parse date
                        dateTimeStr = dateTimeStr.Replace(",", "");
                        {
							const string dateFormat = "MMM d yyyy hh:mm tt";
							try
							{
								DateTime dateTime = DateTime.ParseExact(dateTimeStr, dateFormat, CultureInfo.InvariantCulture);
								Utils.FillPropertiesFromDateTime(dateTime, sp);
							}
							catch (Exception e)
							{
								Console.WriteLine("Warning :: cannot parse date :: " + e.Message);
                                Console.WriteLine("Info :: date string :: " + dateTimeStr);
							}
						}
					}
					else Console.WriteLine("Warning :: unable to extact submission date");

                    // extract description
                    const string key_desc = @"<div class=""submission-description user-submitted-links"">";
                    cpage = cpage.Substring(cpage.IndexOf(key_desc, StringComparison.Ordinal));
                    cpage = cpage.Substring(0,
                                            cpage.IndexOf(key_enddiv, cpage.IndexOf(key_enddiv, StringComparison.Ordinal) + 1,
                                                          StringComparison.Ordinal) + key_enddiv.Length);
                    cpage = cpage.Replace("href=\"/", "href=\"https://furaffinity.net/");
                    cpage = cpage.Replace("src=\"//", "src=\"https://");

                    cpage = @"<div class=""submission-description-container link-override"">
                        <div class=""submission-title"">
                            <h2 class=""submission-title-header"">{{{title}}}</h2>
                            Posted {{{date}}}
                        </div><hr>".Replace("{{{title}}}", sp.TITLE).Replace("{{{date}}}", sub_date_strong) + cpage;
                }

                sp.ARTIST = sp.URL.Substring(sp.URL.LastIndexOf(@"/art/") + 5);
                sp.ARTIST = sp.ARTIST.Substring(0, sp.ARTIST.IndexOf('/'));
                sp.FILEFULL = sp.URL.Substring(sp.URL.LastIndexOf('/') + 1);
				sp.FILEFULL = Utils.StripIllegalFilenameChars(sp.FILEFULL);
                sp.FILEID = sp.FILEFULL.Substring(0, sp.FILEFULL.IndexOf('.'));
                if (sp.FILEFULL.IndexOf('_') >= 0) // valid filename (some names on FA are corrupted and contain nothing but '.' after ID)
                {
                    sp.FILEPART = sp.FILEFULL.Substring(sp.FILEFULL.IndexOf('_') + 1);
                    if (sp.FILEFULL.LastIndexOf('.') >= 0) // has extension
                    {
                        sp.EXT = (sp.FILEFULL + " ").Substring(sp.FILEFULL.LastIndexOf('.') + 1).TrimEnd();
                        if (sp.EXT.CompareTo("") == 0)
                            sp.EXT = @"jpg";
                    }
                    else
                    {
                        sp.EXT = @"jpg";
                    }
                }
                else
                {
                    sp.FILEPART = @"unknown.jpg";
                    sp.EXT = @"jpg";
                }

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
                    Console.WriteLine("Downloading: " + sp.URL);
                    using (
                        Stream contentStream = await (
                            await http.GetAsync(sp.URL, HttpCompletionOption.ResponseHeadersRead)
                        ).Content.ReadAsStreamAsync(),
                        stream = new FileStream(
                            fnamefull,
                            FileMode.Create, FileAccess.Write, FileShare.None, 1024 * 1024 /*Mb*/, true
                        )
                    )
                    {
                        await ReadNetworkStream(contentStream, stream, 5000);
                        // await contentStream.CopyToAsync(stream); // this works, but may hang forever in case of network errors
                        SubmissionsDB.DB.AddSubmission(uint.Parse(subId));
                    }
                }
                catch (Exception E)
                {
                    // write error message
                    if (E is ObjectDisposedException)
                    {
                        Console.WriteLine("Network error (data receive timeout)");
                    }
                    else
                    {
                        Console.WriteLine("GET request error (file " + sp.FILEID + "): " + E.Message);
                    }
                    // remove incomplete download
                    if (File.Exists(fnamefull))
                    {
                        File.Delete(fnamefull);
                    }
                    // try again or abort operation
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
                Console.WriteLine("Done: #" + subId);
                TaskbarProgress.SetValue(currentConsoleHandle, subs.Count - i, subs.Count);
                res.processedPerfectly++;
            }

            // writing results
            try
            {
                if (res.failedToGetPage.Count > 0 || res.failedToDownload.Count > 0)
                {
                    File.WriteAllLines(Path.Combine(GlobalSettings.Settings.systemPath, "get_sub_page_failed.log"), res.failedToGetPage);
                    File.WriteAllLines(Path.Combine(GlobalSettings.Settings.systemPath, "download_failed.log"), res.failedToGetPage);
                }
            }
            catch (Exception E)
            {
                Console.WriteLine("Failed to save list of subs with issues: " + E.Message);
            }
            // save DB
            SubmissionsDB.Save();
            // stop progress indicating
            TaskbarProgress.SetState(currentConsoleHandle, TaskbarProgress.TaskbarStates.NoProgress);
            // return result, actually
            return res;
        }
        
        public async Task<ProcessingResults> ProcessGenericUrl(string link, bool needDesciption)
        {
            List<string> subs = new List<string>();
            if (link.Length > 0 && link[0] == '$')
            {
                // load URLs list from a file
                string listFile = link.Substring(1);
                if (!File.Exists(listFile))
                {
                    throw new Exception ("Referenced links list file seems unreachable.");
                }
                try
                {
                    string[] urls = File.ReadAllLines(listFile);
                    foreach (string url in urls)
                    {
                        string urlFixed = url.TrimEnd(" /".ToCharArray()); // not expecting C# 7.3+ compiler, so can't just assign to "url"
                        if (urlFixed.Length > 0)
                        {
                            List<string> subListFromUrl = null;
                            subListFromUrl = await CreateSubmissionsListFromGallery(urlFixed);
                            if (subListFromUrl != null) subs.AddRange(subListFromUrl);
                        }
                    }
                }
                catch (Exception E)
                {
                    throw new Exception("Error: " + Environment.NewLine + E.Message);
                }
            }
            else
            {
                // single URL, http(s)
                List<string> subListFromUrl = null;
                subListFromUrl = await CreateSubmissionsListFromGallery(link);
                if (subListFromUrl != null) subs.AddRange(subListFromUrl);
            }
            // save submissions list
            try
            {
                Console.WriteLine("Saving submissions list...");
                File.WriteAllLines(Path.Combine(GlobalSettings.Settings.systemPath, "latest_subs.log"), subs);
                Console.WriteLine("Submissions list saved to \"latest_subs.log\"");
            }
            catch (Exception E)
            {
                Console.WriteLine("Failed to save submissions list: " + E.Message);
            }
            // save images/descriptions etc.
            return await ProcessSubmissionsList(subs, needDesciption);
        }
    }
}
