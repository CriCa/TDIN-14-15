using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public abstract class Functions
    {
        private static string DATE_FORMAT = "dd-MM-yyyy hh:mm";
        private static DateTime UNIX_EPOCH = new DateTime(1970, 1, 1);
        public static string getCurrentDate()
        {
            return getStringFromDate(DateTime.Now);
        }

        public static string getStringFromDate(DateTime d)
        {
            DateTime date = DateTime.ParseExact(d.ToString(DATE_FORMAT), DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal);
            return date.ToString(DATE_FORMAT);
        }

        public static DateTime getDate(string d)
        {
            return DateTime.ParseExact(d, DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal);
        }
    }
}
