using System;

namespace Toolroom.ApiHelper
{
    public static class DateTimeExtensions
    {
        private const string DefaultTimeZoneId = "W. Europe Standard Time";
        public static DateTime FromUtcToLocalDateTime(this DateTime utcDateTime, string timeZoneId = DefaultTimeZoneId)
        {
            TimeZoneInfo tz;
            tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, tz);
        }
    }
}