using System;
using System.Linq;

namespace Fitmeplan.Common
{
    public static class DateTimeExtension
    {
        /// <summary>
        /// Returns date in "GMT Standard Time" time zone for Windows or "Europe/London" time zone for Linux.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime GetDateInDefaultTimeZone(this DateTime dateTime)
        {
            var winDefaultTimeZoneId = "GMT Standard Time";
            var linDefaultTimeZoneId = "Europe/London";
            var timeZones = TimeZoneInfo.GetSystemTimeZones();

            var defaultTimeZone = timeZones.FirstOrDefault(x => x.Id == winDefaultTimeZoneId) ??
                                  timeZones.FirstOrDefault(x => x.Id == linDefaultTimeZoneId) ??
                                  TimeZoneInfo.CreateCustomTimeZone("Custom GMT Time", TimeSpan.FromHours(0), "Custom GMT Time", "Custom GMT Time");

            return TimeZoneInfo.ConvertTimeFromUtc(dateTime, defaultTimeZone).Date;
        }

        /// <summary>
        /// Returns date in the specific time zone.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="timeZoneName"></param>
        /// <returns></returns>
        public static DateTime GetDateInSpecificTimeZone(this DateTime dateTime, string timeZoneName)
        {
            var systemTimeZones = TimeZoneInfo.GetSystemTimeZones();

            var timezone = systemTimeZones.FirstOrDefault(x => x.Id == timeZoneName);
            if (timezone == null)
            {
                return dateTime.GetDateInDefaultTimeZone();
            }
            else
            {
                return TimeZoneInfo.ConvertTimeFromUtc(dateTime, timezone).Date;
            }
        }

        /// <summary>
        /// Returns date in "GMT Standard Time" time zone for Windows or "Europe/London" time zone for Linux.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime GetDateTimeInDefaultTimeZone(this DateTime dateTime)
        {
            var winDefaultTimeZoneId = "GMT Standard Time";
            var linDefaultTimeZoneId = "Europe/London";
            var timeZones = TimeZoneInfo.GetSystemTimeZones();

            var defaultTimeZone = timeZones.FirstOrDefault(x => x.Id == winDefaultTimeZoneId) ??
                                  timeZones.FirstOrDefault(x => x.Id == linDefaultTimeZoneId) ??
                                  TimeZoneInfo.CreateCustomTimeZone("Custom GMT Time", TimeSpan.FromHours(0), "Custom GMT Time", "Custom GMT Time");

            return TimeZoneInfo.ConvertTimeFromUtc(dateTime, defaultTimeZone);
        }

        /// <summary>
        /// Returns date time in the specific time zone.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="timeZoneName"></param>
        /// <returns></returns>
        public static DateTime GetDateTimeInSpecificTimeZone(this DateTime dateTime, string timeZoneName)
        {
            var systemTimeZones = TimeZoneInfo.GetSystemTimeZones();

            var timezone = systemTimeZones.FirstOrDefault(x => x.Id == timeZoneName);
            if (timezone == null)
            {
                return dateTime.GetDateTimeInDefaultTimeZone();
            }
            else
            {
                return TimeZoneInfo.ConvertTimeFromUtc(dateTime, timezone);
            }
        }
    }
}
