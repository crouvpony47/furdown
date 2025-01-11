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

        public static void OpenUrl(string urlToOpen)
        {
            try
            {
                System.Diagnostics.Process.Start(urlToOpen);
            }
            catch (Exception)
            {
                Console.WriteLine("Could not open URL in the default browser:\n" + urlToOpen);
            }
        }
    }
}
