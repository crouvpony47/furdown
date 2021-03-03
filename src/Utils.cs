using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace furdown
{
    public class Utils
    {
		public static void FillPropertiesFromDateTime(DateTime dateTime, SubmissionProps props)
		{
			props.PDYEAR = dateTime.Year.ToString("D4");
			props.PDMON  = dateTime.Month.ToString("D2");
			props.PDDAY  = dateTime.Day.ToString("D2");
			props.PDHOUR = dateTime.Hour.ToString("D2");
			props.PDMIN =  dateTime.Minute.ToString("D2");         
		}

        public static string StripIllegalFilenameChars(string name)
		{
			return string.Join("_", name.Split(
				System.IO.Path.GetInvalidFileNameChars() //,
				// StringSplitOptions.RemoveEmptyEntries
			));
		}

        public static string FileHash(string path)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = System.IO.File.OpenRead(path))
                {
                    return System.Text.Encoding.Default.GetString(md5.ComputeHash(stream));
                }
            }
        }

        public class EmbeddedIeUtils
        {
            [DllImport("urlmon.dll", CharSet = CharSet.Ansi)]
            private static extern int UrlMkSetSessionOption(int dwOption, string pBuffer, int dwBufferLength, int dwReserved);

            const int URLMON_OPTION_USERAGENT = 0x10000001;
            const string ua = "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; Touch; rv:11.0) like Gecko";

            static string GetUserAgent()
            {
                string envUa = Environment.GetEnvironmentVariable("FURDOWN_USERAGENT");
                return envUa == null ? ua : envUa;
            }

            public static void SetKnownUserAgent()
            {
                var userAgent = GetUserAgent();
                UrlMkSetSessionOption(URLMON_OPTION_USERAGENT, userAgent, userAgent.Length, 0);
            }

            public static string GetKnownUserAgentValue()
            {
                return GetUserAgent();
            }
        }
    }
}
