using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace furdown
{
    internal static class CookiesStorage
    {
        private static string cookieString = null;
        private static string assocUserAgent = "Mozilla/5.0";

        public static string GetCookieString()
        {
            string envCookies = Environment.GetEnvironmentVariable("FURDOWN_COOKIES");
            if (envCookies != null)
            {
                return envCookies;
            }
            return cookieString;
        }

        public static void SetCookieString(string newCookies)
        {
            cookieString = newCookies;
        }

        public static string GetAssociatedUserAgent()
        {
            string envUA = Environment.GetEnvironmentVariable("FURDOWN_USERAGENT");
            if (envUA != null)
            {
                return envUA;
            }
            return assocUserAgent;
        }

        public static void SetAssociatedUserAgent(string newUserAgent)
        {
            assocUserAgent = newUserAgent;
        }
    }
}
