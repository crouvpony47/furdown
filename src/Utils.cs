using System;

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
			return string.Concat(name.Split(
				System.IO.Path.GetInvalidFileNameChars(),
				StringSplitOptions.RemoveEmptyEntries
			));
		}
    }
}
