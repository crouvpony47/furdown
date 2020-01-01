using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace furdown
{
    class UpdatesChecker
    {
        internal static async Task<bool> CheckRemoteVersion()
        {
            try
            {
                var httph = new HttpClientHandler();
                var http = new HttpClient(httph);
                http.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                serializer.MaxJsonLength = 2097152; // 2 Mb

                string[] thisVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString().Split('.');

                var jsonString = await http.GetStringAsync(@"https://api.github.com/repos/crouvpony47/furdown/releases");
                var jsonRoot = (object[])serializer.DeserializeObject(jsonString);

                if (jsonRoot.Length == 0)
                {
                    Console.WriteLine("Updater :: Warning :: no releases found on GitHub");
                    return false;
                }

                foreach (var releaseJson in jsonRoot)
                {
                    var releaseDict = releaseJson as Dictionary<string, object>;
                    var tag = (string)releaseDict["tag_name"];
                    tag = tag.Replace("v.", "");
                    var prerelease = (bool)releaseDict["prerelease"];
                    if (prerelease)
                    {
                        Console.WriteLine("Updater :: Note :: found prerelease, " + tag);
                        continue;
                    }

                    Console.WriteLine("Updater :: Note :: found release, " + tag);

                    var remoteVersion = tag.Split('.');
                    var versionComp = thisVersion.Zip(remoteVersion, (tvp, rvp) =>
                    {
                        return string.Compare(tvp, rvp, StringComparison.Ordinal);
                    });

                    foreach (int c in versionComp)
                    {
                        if (c < 0)
                        {
                            Console.WriteLine("Updater :: Note :: an update is available");
                            return true;
                        }
                        else if (c > 0)
                        {
                            Console.WriteLine("Updater :: Note :: current version is newer than the latest released one");
                            return false;
                        }
                    }
                    return false;
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("Updater :: Error :: " + exc.Message);
            }
            return false;
        }
    }
}
