//#define DEBUG_PRINT_ALL_TEMPLATE_VALS

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
        #if !furdown_portable_core
        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool InternetGetCookieEx(string pchURL, string pchCookieName, StringBuilder pchCookieData, ref uint pcchCookieData, int dwFlags, IntPtr lpReserved);
        #endif
        private const int INTERNET_COOKIE_HTTPONLY = 0x00002000;
        #endregion

        public string defaultUserId = "";

        /// <summary>
        /// Gets all, including http-only, cookies from WebBrowser component
        /// </summary>
        /// <param name="uri">URI which is used to get the cookies for</param>
        /// <returns></returns>
        private static string GetGlobalCookies(string uri)
        {
            string envCookies = Environment.GetEnvironmentVariable("FURDOWN_COOKIES");
            if (envCookies != null) return envCookies;

            #if !furdown_portable_core
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
            #else
            return null;
            #endif
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
                        var defUsrMatch = Regex.Match(cpage, @"href=""/commissions/(.+?)/""", RegexOptions.CultureInvariant);
                        if (defUsrMatch.Success)
                        {
                            defaultUserId = defUsrMatch.Groups[1].Value;
                            //Console.WriteLine("Hi " + defaultUserId);
                        }
                        else
                        {
                            Console.WriteLine("Warning :: could not determine the username to use for the default target gallery url");
                        }
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

        private void TryFixGalleryUrl(ref string gallery)
        {
            // ensure that the url starts with https://www.furaffinity.net and has no trailing `/`
            gallery = gallery
                .Trim(" /".ToCharArray())
                .Replace("http://", "https://");
            if (!gallery.StartsWith("https://"))
                gallery = "https://" + gallery;
            gallery = gallery
                .Replace("https://furaffinity.net", "https://www.furaffinity.net");
            // validate url
            // - folder
            var vFldMatch = Regex.Match(gallery, @"(https://www.furaffinity.net/gallery/[^/]+/folder/[0-9]+)(/[^/]+){0,1}");
            if (vFldMatch.Success)
            {
                var fldVanityName = vFldMatch.Groups[2].Value;
                gallery = vFldMatch.Groups[1].Value + (string.IsNullOrEmpty(fldVanityName) ? "/folder" : fldVanityName);
                return;
            }
            // - gallery/scraps/favs
            var vGalMatch = Regex.Match(gallery, @"https://www.furaffinity.net/(gallery|scraps|favorites)/[^/]+");
            if (vGalMatch.Success)
            {
                gallery = vGalMatch.Value;
                return;
            }
            // - watches / submission inbox
            if (gallery.StartsWith("https://www.furaffinity.net/msg/submissions"))
            {
                gallery = "https://www.furaffinity.net/msg/submissions";
                return;
            }
            // - loose submission
            var vLooseSubMatch = Regex.Match(gallery, @"https://www.furaffinity.net/view/[0-9]+");
            if (vLooseSubMatch.Success)
            {
                gallery = vLooseSubMatch.Value;
                return;
            }
            // something else
            throw new Exception(string.Format("Cannot process {0}\n"
                + "Supported URLs are: gallery (gallery folder), "
                + "scraps, favorites, submission, submissions inbox.", gallery));
        }

        public async Task<List<string>> CreateSubmissionsListFromGallery(string gallery)
        {
            List<string> lst = new List<string>();

            // fix non-ideal links first
            TryFixGalleryUrl(ref gallery);
            // and handle the redundant case of a single submission link
            if (gallery.StartsWith("https://www.furaffinity.net/view/"))
            {
                lst.Add(gallery.Replace("https://www.furaffinity.net/view/", ""));
                return lst;
            }

            Console.WriteLine("Building submissions list for " + gallery);

            string commonAttributes = (gallery.Contains("/scraps/") ? "@s" : "");
            TaskbarProgress.SetState(currentConsoleHandle, TaskbarProgress.TaskbarStates.Indeterminate);
            int page = 0;
            long favNextId = -1;
            long subInboxStamp = long.MaxValue;
            int subInboxSpp = 48;
            string subInboxNewOld = "new";
            while (true)
            {
                page++;
                string pageUrl = gallery + ((page == 1) ? "/" : ("/" + page.ToString()));
                if (favNextId >= 0)
                {
                    pageUrl = gallery + "/" + favNextId.ToString() + "/next";
                }
                if (subInboxStamp != long.MaxValue)
                {
                    pageUrl = gallery + "/" + string.Format("{0}~{1}@{2}/", subInboxNewOld, subInboxStamp, subInboxSpp);
                }
                const string submIdAndFidRegex = @"figure id=""sid-(.+?)"".+?@.+?-(.+?)[.""]"; // group 1: subm id, group 2: file id
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
                do
                {
                    var nextMatch = Regex.Match(cpage, submIdAndFidRegex, RegexOptions.CultureInvariant);
                    if (nextMatch.Success)
                    {
                        uint sid = 0, fid = 0;
                        if (!uint.TryParse(nextMatch.Groups[1].Value, out sid))
                        {
                            Console.WriteLine("Error :: failed to parse a submission ID in the list.");
                            break;
                        }
                        if (!uint.TryParse(nextMatch.Groups[2].Value, out fid))
                        {
                            Console.WriteLine(string.Format("Warning :: file ID could not be determined for submission {0}", sid));
                        }
                        lst.Add(string.Format("{0}#{1}", sid, fid) + commonAttributes);
                        counter++;
                        string key = nextMatch.Groups[0].Value;
                        cpage = cpage.Substring(cpage.IndexOf(key, StringComparison.Ordinal) + key.Length);
                    }
                    else break;
                } while (true);
                if (counter == 0)
                {
                    Console.WriteLine("Reached an empty page. Total elements found: " + lst.Count.ToString());
                    break;
                }

                // if we're browsing favs, page numbering scheme is different
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

                // submissions inbox
                if (pageUrl.Contains("/msg/submissions/"))
                {
                    const string nextPrevBtnRe = @"<a.*?(new|old)~(.+?)@(.*?)/.*?/a>";
                    MatchCollection npMatches = Regex.Matches(cpage, nextPrevBtnRe);
                    bool hasGoodNextPage = false;
                    foreach (Match npMatch in npMatches)
                    {
                        if (!npMatch.Value.Contains("more")) continue;
                        try
                        {
                            var subInboxStampCand = long.Parse(npMatch.Groups[2].Value);
                            // note: In the following condition the first branch will be used
                            //       for the first page regardless of the newFirst/oldFirst order.
                            //       This is intentional, since any timestamp will be < MaxInt, and
                            //       we can thus get away with the same initial value for 'subInboxStamp'
                            if ((subInboxNewOld == "new" && subInboxStampCand < subInboxStamp)
                                || (subInboxNewOld == "old" && subInboxStampCand > subInboxStamp))
                            {
                                subInboxStamp = subInboxStampCand;
                                subInboxSpp = int.Parse(npMatch.Groups[3].Value);
                                subInboxNewOld = npMatch.Groups[1].Value;
                                hasGoodNextPage = true;
                            }
                        }
                        catch
                        {
                            Console.WriteLine("Warning :: can't get the next page URL");
                            break;
                        }
                    }
                    if (!hasGoodNextPage)
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

        public async Task<ProcessingResults> ProcessSubmissionsList(List<string> subs, bool needDescription, bool updateMode = false)
        {
            Console.WriteLine("Processing submissions list...");
            ProcessingResults res = new ProcessingResults();
            // iterate over all the submissions in list
            for (int i = subs.Count - 1; i >= 0; i--)
            {
                // expected format: ID#FileID@attributes
                // everything except for ID is optional
                string subStr = subs[i];
                if (string.IsNullOrEmpty(subStr)) continue; // don't care about empty strings

                string subId;
                uint subIdInt = 0;
                uint subFid = 0;
                uint subInitFid = 0;
                bool aScraps = false;

                const string subIdRegex = @"^(?<id>[0-9]+?)(#(?<fid>[0-9]+?)){0,1}(@(?<attr>.+?)){0,1}$";
                var subIdMatch = Regex.Match(subStr, subIdRegex);
                if (subIdMatch.Success)
                {
                    subId = subIdMatch.Groups["id"].Value;
                    uint.TryParse(subId, out subIdInt);
                    if (subIdMatch.Groups["fid"].Success)
                    {
                        uint.TryParse(subIdMatch.Groups["fid"].Value, out subFid);
                    }
                    /// Attributes section has only been used for (terrible) scraps detection;
                    /// a better method is now implemented, making the section useless
                    //if (subIdMatch.Groups["attr"].Success)
                    //{
                    //    string attributes = subIdMatch.Groups["attr"].Value;
                    //    if (attributes.Contains("s")) aScraps = true;
                    //}
                }
                else
                {
                    Console.WriteLine("Error :: Malformed submission ID: " + subStr);
                    continue;
                }
                
                uint dbSubFid = SubmissionsDB.DB.GetFileId(subIdInt);
                bool dbSubExists = SubmissionsDB.DB.Exists(subIdInt);

                Console.WriteLine(string.Format("> Processing submission {0} {1}",
                    subId,
                    subFid > 0 ? string.Format("(file id {0})", subFid) : ""
                ));

                // Skip submissions that can be skipped without making any network requests
                try
                {
                    if (dbSubExists && GlobalSettings.Settings.downloadOnlyOnce)
                    {
                        // can skip at lowest cost if either:
                        // * not in update mode
                        // * file ID is known and matches the one stored in the DB
                        if ((!updateMode) || (updateMode && dbSubFid == subFid && dbSubFid != 0))
                        {
                            Console.WriteLine("Skipped (present in DB)");
                            continue;
                        }
                        else
                        {
                            Console.WriteLine("Submission is present in the DB, but may have been updated; re-checking~");
                        }
                    }
                }
                catch
                {
                    Console.WriteLine("Unexpected error (DB presence check failed)!");
                    continue;
                }

                // get submission page
                string subUrl = "https://www.furaffinity.net/view/" + subId;
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
                var downbtnkeys = new string[]{
                    "<a href=\"//d.facdn.net/", "<a href=\"//d2.facdn.net/",
                    "<a href=\"//d.furaffinity.net/"
                };
                SubmissionProps sp = new SubmissionProps();
                sp.SUBMID = subId;
                int keypos = -1;
                foreach (var downbtnkey in downbtnkeys)
                {
                    keypos = cpage.IndexOf(downbtnkey, StringComparison.Ordinal);
                    if (keypos >= 0)
                    {
                        break;
                    }
                }
                if (keypos < 0)
                {
                    Console.WriteLine("[Error] got page, but it doesn't contain any download links.");
                    res.failedToGetPage.Add(subId);
                    continue;
                }
                cpage = cpage.Substring(keypos);
                cpage = cpage.Substring(cpage.IndexOf("/", StringComparison.Ordinal));
                sp.URL = "https:"
                    + cpage.Substring(0, cpage.IndexOf("\"", StringComparison.Ordinal));

                #region download URL parsing
                bool extensionInvalid = false; // future use, possibly come up with an extension that makes sense on a case by case basis
                const string urlComponentsRegex = @"\/art\/(?<artist>.+?)\/.*?(?<curfid>\d+?)\/(?<fid>.+?)\.(?<fname>.*)$";
                var urlCompMatch = Regex.Match(sp.URL, urlComponentsRegex);
                if (urlCompMatch.Success)
                {
                    sp.ARTIST = urlCompMatch.Groups["artist"].Value;
                    sp.CURFILEID = urlCompMatch.Groups["curfid"].Value;
                    uint.TryParse(sp.CURFILEID, out subFid);
                    sp.FILEID = urlCompMatch.Groups["fid"].Value;
                    uint.TryParse(sp.FILEID, out subInitFid);
                    string filename = urlCompMatch.Groups["fname"].Value;
                    /// original filename usually follows this pattern:
                    ///     $file_id.$artist_originalFileName.ext
                    ///             [^ "fname" group value       ]
                    /// however, some old (~2006) submissions use this pattern instead:
                    ///     $file_id.$artist.originalFileName.ext
                    /// it is also quite common for the fname to be blank, i.e.
                    ///     $file_id.
                    /// in this case we have no choice but to come up with our own name
                    var fnameCheckMatch = Regex.Match(filename, string.Format(@"^{0}[_.](.+)", Regex.Escape(sp.ARTIST)));
                    if (fnameCheckMatch.Success)
                    {
                        var filepart = fnameCheckMatch.Groups[1].Value;
                        if (filepart.EndsWith(".") || !filepart.Contains(".")) // no extension or an empty one
                        {
                            extensionInvalid = true;
                            Console.WriteLine("Info :: missing filename extension, assuming .jpg");
                            if (filepart.EndsWith("."))
                                filepart = filepart.Substring(0, filepart.Length - 1) + ".jpg";
                            else
                                filepart = filepart + ".jpg";
                        }
                        var filepartDotSplit = filepart.Split(new char[] { '.' });
                        sp.FILEPART = filepart;
                        sp.FILEPARTNE = string.Join(".", filepartDotSplit.Take(filepartDotSplit.Length - 1));
                        sp.EXT = Utils.StripIllegalFilenameChars(filepartDotSplit.Last());
                    }
                    else // completely broken filenames get replaced with "unknown.jpg"
                    {
                        Console.WriteLine("Info :: broken filename detected, replacing with \"unknown.jpg\"");
                        sp.FILEPART = "unknown.jpg";
                        sp.FILEPARTNE = "unknown";
                        sp.EXT = "jpg";
                        extensionInvalid = true;
                    }

                    sp.FILEFULL = sp.FILEID + "." + sp.ARTIST + "_" + sp.FILEPART;

                    sp.FILEFULL = Utils.StripIllegalFilenameChars(sp.FILEFULL);
                    sp.FILEPART = Utils.StripIllegalFilenameChars(sp.FILEPART);
                }
                else
                {
                    Console.WriteLine("Error: could not make sense of the URL for submission " + subId);
                    res.failedToDownload.Add(subId);
                    continue;
                }
                #endregion

                #region scraps detection, submission date, title and description
                {
                    const string key_title = @"<div class=""submission-title"">";
                    const string key_enddiv = "</div>";
                    var submTitlePos = cpage.IndexOf(key_title, StringComparison.Ordinal);

                    // scraps check: if there is a link to /$user/scraps before the submission title, it's in scraps
                    var scrapsCheckMatch = Regex.Match(cpage, string.Format(@"href=""/scraps/{0}/""", Regex.Escape(sp.ARTIST)));
                    if (scrapsCheckMatch.Success && scrapsCheckMatch.Index < submTitlePos)
                    {
                        Console.WriteLine("Location: scraps");
                        aScraps = true;
                    }
                    else
                    {
                        Console.WriteLine("Location: main gallery");
                        aScraps = false;
                    }

                    Utils.FillPropertiesFromDateTime(DateTime.Now, sp); // set Now as a fallback date
                    sp.TITLE = "Unknown"; // fallback title

                    // title
                    cpage = cpage.Substring(submTitlePos);
                    string sub_title_div = cpage.Substring(0,
                                                           cpage.IndexOf(key_enddiv, cpage.IndexOf(key_enddiv, StringComparison.Ordinal) + 1,
                                                                         StringComparison.Ordinal) + key_enddiv.Length);
                    var titleMatch = Regex.Match(sub_title_div, "<h2><p>(.+?)</p></h2>", RegexOptions.CultureInvariant);
                    if (titleMatch.Success)
                    {
                        sp.TITLE = Utils.StripIllegalFilenameChars(System.Net.WebUtility.HtmlDecode(titleMatch.Groups[1].Value));
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
                #endregion

                // apply template(s)
                string fname = GlobalSettings.Settings.filenameTemplate;
                string dfname = GlobalSettings.Settings.descrFilenameTemplate;
                var scrapsTemplate = aScraps ? GlobalSettings.Settings.scrapsTemplateActive : GlobalSettings.Settings.scrapsTemplatePassive;
                fname = fname.Replace("%SCRAPS%", scrapsTemplate);
                dfname = dfname.Replace("%SCRAPS%", scrapsTemplate);
                foreach (FieldInfo fi
                    in sp.GetType().GetFields(
                    BindingFlags.Instance | BindingFlags.Public).ToArray()
                )
                {
                    if (fi.FieldType == typeof(string))
                    {
                        fname = fname.Replace("%" + fi.Name + "%", (string)fi.GetValue(sp));
                        dfname = dfname.Replace("%" + fi.Name + "%", (string)fi.GetValue(sp));
#if DEBUG_PRINT_ALL_TEMPLATE_VALS
                        // debug only: output all template values:
                        Console.WriteLine(string.Format("+++ {0} = {1}", "%" + fi.Name + "%", (string)fi.GetValue(sp)));
#endif
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

                var fileExists = File.Exists(fnamefull);

                Console.WriteLine("target filename: " + fname + (fileExists ? " (exists)" : ""));

                // at this point we have the actual file ID, and can skip downloading based on that
                if (GlobalSettings.Settings.downloadOnlyOnce)
                {
                    if ((!updateMode) && fileExists) // checked earlier: && !dbSubExists
                    {
                        if (subInitFid != subFid)
                        {
                            Console.WriteLine(string.Format(
                                "Note :: submission {0} exists locally, but could've been updated\n"
                                + "consider running this task in update mode", subId
                                ));
                            SubmissionsDB.DB.AddSubmission(subIdInt);
                        }
                        else
                        {
                            Console.WriteLine("Already exists, continuing~");
                            SubmissionsDB.DB.AddSubmissionWithFileId(subIdInt, subFid);
                        }
                        continue;
                    }
                    // this exact check can also be found before, it is repeated here for cases
                    // when subFid was not known before the submission page request
                    if (updateMode && dbSubFid == subFid && dbSubFid != 0)
                    {
                        Console.WriteLine("Already downloaded, continuing~");
                        continue;
                    }
                }
                else // not `download only once`
                {
                    if ((!updateMode) && fileExists)
                    {
                        if (subInitFid != subFid)
                        {
                            SubmissionsDB.DB.AddSubmission(subIdInt);
                        }
                        else
                        {
                            SubmissionsDB.DB.AddSubmissionWithFileId(subIdInt, subFid);
                        }
                        Console.WriteLine("Already exists, continuing~");
                        continue;
                    }
                    if (updateMode && fileExists && dbSubFid == subFid && dbSubFid != 0)
                    {
                        Console.WriteLine("Already exists, continuing~");
                        continue;
                    }
                }

                // if we got here, there was no reason to skip the download
                bool mayBeUselessDownload = false;
                string oldFileHash = "";
                if (fileExists)
                {
                    Console.WriteLine(string.Format("subfid {0}   dbsf {1}", subFid, dbSubFid));
                    oldFileHash = Utils.FileHash(fnamefull);
                    fnamefull = Path.Combine(GlobalSettings.Settings.downloadPath,
                                             string.Format("{1} [v.{0}].{2}", subFid, fname, sp.EXT));
                    if (!(subFid != dbSubFid && dbSubFid != 0))
                    {
                        Console.WriteLine("Info :: stored metadata is insufficient; downloading a remote file to compare aganst local");
                        mayBeUselessDownload = true;
                    }
                }
                

                // download file
                int fattempts = 3;
                fbeforeawait:
                try
                {
                    Console.WriteLine("Downloading: " + sp.URL);
                    // "?" can only be in the URL if the user named their submission this way
                    // it WILL be mistreated as an URL parameter, but this dirty hack with explicit replacement fixes it
                    using (var response = await http.GetAsync(sp.URL.Replace("?", "%3F"), HttpCompletionOption.ResponseHeadersRead))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            throw new Exception(string.Format("HTTP error: {0}", response.StatusCode));
                        }
                        using (
                            Stream contentStream = await response.Content.ReadAsStreamAsync(),
                            stream = new FileStream(
                                fnamefull,
                                FileMode.Create, FileAccess.Write, FileShare.None, 1024 * 1024 /*Mb*/, true
                            )
                        )
                        {
                            await ReadNetworkStream(contentStream, stream, 5000);
                            // await contentStream.CopyToAsync(stream); // this works, but may hang forever in case of network errors
                        }
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

                SubmissionsDB.DB.AddSubmissionWithFileId(subIdInt, subFid);
                if (mayBeUselessDownload)
                {
                    var newFileHash = Utils.FileHash(fnamefull);
                    if (newFileHash == oldFileHash)
                    {
                        Console.WriteLine("Note :: existing version matches the one on the server, removing a duplicate");
                        File.Delete(fnamefull);
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
                    File.WriteAllLines(Path.Combine(GlobalSettings.Settings.systemPath, "download_failed.log"), res.failedToDownload);
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
        
        public async Task<ProcessingResults> ProcessGenericUrl(string link, bool needDesciption, bool updateMode = false)
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
                    TaskbarProgress.SetState(currentConsoleHandle, TaskbarProgress.TaskbarStates.Indeterminate);
                    TaskbarProgress.LockState();
                    foreach (string url in urls)
                    {
                        string urlFixed = url.TrimEnd(" /".ToCharArray()); // not expecting C# 7.3+ compiler, so can't just assign to "url"
                        if (urlFixed.Length > 0)
                        {
                            List<string> subListFromUrl = null;
                            subListFromUrl = await CreateSubmissionsListFromGallery(urlFixed);
                            if (subListFromUrl != null)
                            {
                                subs.AddRange(subListFromUrl);
                                if (subListFromUrl.Count == 0)
                                {
                                    Console.WriteLine("Note :: no submissions found for " + urlFixed);
                                }
                            }
                        }
                    }
                    TaskbarProgress.UnlockState();
                    TaskbarProgress.SetState(currentConsoleHandle, TaskbarProgress.TaskbarStates.NoProgress);
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
            return await ProcessSubmissionsList(subs, needDesciption, updateMode);
        }
    }
}
