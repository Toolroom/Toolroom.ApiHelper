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
        public static DateTime? FromUtcToLocalDateTime(this DateTime? utcDateTime, string timeZoneId = DefaultTimeZoneId)
        {
            if (!utcDateTime.HasValue)
                return null;
            TimeZoneInfo tz;
            tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime.Value, tz);
        }

        public static DateTime FromLocalToUtcDateTime(this DateTime localDateTime, string timeZoneId = DefaultTimeZoneId)
        {
            TimeZoneInfo tz;
            tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTime(localDateTime, tz, TimeZoneInfo.Utc);
        }

        public static DateTime? FromLocalToUtcDateTime(this DateTime? localDateTime, string timeZoneId = DefaultTimeZoneId)
        {
            if (!localDateTime.HasValue)
            {
                return null;
            }
            TimeZoneInfo tz;
            tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTime(localDateTime.Value, tz, TimeZoneInfo.Utc);
        }

        public static DateTime ConvertToUnspecifiedDateTime(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, DateTimeKind.Unspecified);
        }
    }
}