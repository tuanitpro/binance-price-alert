using System;
using System.Globalization;

namespace ColorLife.Core.Helper
{
    public static class ConvertType
    {
        public static int ToInt(object obj, int defaultValue)
        {
            try { return int.Parse(obj.ToString()); }
            catch { return defaultValue; }
        }

        public static int ToInt(this object obj)
        {
            return ToInt(obj, -1);
        }

        public static double ToDouble(object obj, double defaultValue)
        {
            try { return double.Parse(obj.ToString()); }
            catch { return defaultValue; }
        }

        public static double ToDouble(this object obj)
        {
            return ToDouble(obj, -1);
        }

        public static float ToFloat(object obj, float defaultValue)
        {
            try { return float.Parse(obj.ToString()); }
            catch { return defaultValue; }
        }

        public static float ToFloat(this object obj)
        {
            return ToFloat(obj, -1);
        }

        public static short ToShort(object obj, short defaultValue)
        {
            try { return short.Parse(obj.ToString()); }
            catch { return defaultValue; }
        }

        public static short ToShort(this object obj)
        {
            return ToShort(obj, -1);
        }

        public static long ToLong(object obj, long defaultValue)
        {
            try { return long.Parse(obj.ToString()); }
            catch { return defaultValue; }
        }

        public static long ToLong(this object obj)
        {
            return ToLong(obj, -1);
        }

        public static DateTime StartOfDate(this DateTime theDate)
        {
            return theDate.Date;
        }

        public static DateTime EndOfDate(this DateTime theDate)
        {
            return theDate.Date.AddDays(1).AddTicks(-1);
        }

        public static DateTime ToDateTime(object obj, DateTime defaultValue)
        {
            try
            {
                return Convert.ToDateTime(obj.ToString());
            }
            catch { return defaultValue; }
        }

        public static DateTime ToDateTime(this object obj)
        {
            return ToDateTime(obj, DateTime.MinValue);
        }

        public static DateTime ToDateTime(this object obj, string culture)
        {
            try
            {
                //  IFormatProvider culture = new System.Globalization.CultureInfo("fr-FR", true);

                CultureInfo cultureX = new CultureInfo(culture, true);
                DateTime dt = DateTime.Now;

                DateTime dt2 = DateTime.Parse(obj.ToString(), cultureX, DateTimeStyles.AssumeLocal);
                DateTime output = new DateTime(dt2.Year, dt2.Month, dt2.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond);
                return output;
            }
            catch
            {
                return ToDateTime(obj, DateTime.MinValue);
            }
        }

        public static DateTime ToDateTimeGTM(this object obj)
        {
            //  var registerDateString = "Fri Aug 26 2016 00:00:00 GMT+0700 (SE Asia Standard Time)";
            var registerDateString = obj.ToString();
            var timeZoneIndex = registerDateString.IndexOf("(", System.StringComparison.Ordinal);

            var dateTimeString = registerDateString.Substring(0, timeZoneIndex - 1);
            var FormattedDate = DateTime.ParseExact(dateTimeString, "ddd MMM dd yyyy HH:mm:ss 'GMT'zzz", System.Globalization.CultureInfo.InvariantCulture);

            return FormattedDate;
        }

        public static DateTime TimeFromUnixTimestamp(this int unixTimestamp)
        {
            DateTime unixYear0 = new DateTime(1970, 1, 1);
            long unixTimeStampInTicks = unixTimestamp * TimeSpan.TicksPerSecond;
            DateTime dtUnix = new DateTime(unixYear0.Ticks + unixTimeStampInTicks);
            return dtUnix;
        }

        public static bool ToBool(object obj, bool defaultValue)
        {
            try { return Convert.ToBoolean(obj.ToString()); }
            catch { return defaultValue; }
        }

        public static bool ToBool(this object obj)
        {
            return ToBool(obj, false);
        }

        public static decimal ToDecimal(object obj, decimal defaultValue)
        {
            try { return Convert.ToDecimal(obj.ToString()); }
            catch { return defaultValue; }
        }

        public static decimal ToDecimal(this object obj)
        {
            return ToDecimal(obj, 0);
        }

        public static T ToEnum<T>(int number)
        {
            return (T)Enum.ToObject(typeof(T), number);
        }

        public static T ToEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value);
        }
    }
}