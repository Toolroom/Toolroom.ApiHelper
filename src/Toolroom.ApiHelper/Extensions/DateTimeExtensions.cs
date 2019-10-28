using System;

namespace Toolroom.ApiHelper
{
    public static class DateTimeExtensions
    {
        private const string DefaultTimeZoneId = "W. Europe Standard Time";

        public static DateTime FromUtcToLocalDateTime(
            this DateTime utcDateTime, 
            string timeZoneId = DefaultTimeZoneId)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, tz);
        }

        public static DateTime? FromUtcToLocalDateTime(
            this DateTime? utcDateTime,
            string timeZoneId = DefaultTimeZoneId)
        {
            if (!utcDateTime.HasValue)
                return null;

            return FromUtcToLocalDateTime(utcDateTime.Value, timeZoneId);
        }

        public static DateTime FromLocalToUtcDateTime(
            this DateTime localDateTime,
            string timeZoneId = DefaultTimeZoneId)
        {
            if (localDateTime.Kind == DateTimeKind.Utc)
                return localDateTime;

            localDateTime = DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified);

            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);          
            return TimeZoneInfo.ConvertTime(localDateTime, tz, TimeZoneInfo.Utc);
        }

        public static DateTime? FromLocalToUtcDateTime(
            this DateTime? localDateTime,
            string timeZoneId = DefaultTimeZoneId)
        {
            if (!localDateTime.HasValue)
                return null;

            return FromLocalToUtcDateTime(localDateTime.Value, timeZoneId);
        }

        public static DateTime ConvertToUnspecifiedDateTime(this DateTime dt)
        {
            return DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);
        }
        public static DateTime? ConvertToUnspecifiedDateTime(this DateTime? dt)
        {
            if (!dt.HasValue)
            {
                return null;
            }
            return ConvertToUnspecifiedDateTime(dt.Value);
        }


        public static DateTime Floor(this DateTime dt, TimeSpan ts)
        {
            long ticks = dt.Ticks / ts.Ticks;
            return new DateTime(ticks * ts.Ticks);
        }

        public static DateTime Round(this DateTime dt, TimeSpan ts)
        {
            long ticks = (dt.Ticks + (ts.Ticks / 2) + 1) / ts.Ticks;
            return new DateTime(ticks * ts.Ticks);
        }

        public static DateTime Ceiling(this DateTime dt, TimeSpan ts)
        {
            long ticks = (dt.Ticks + ts.Ticks - 1) / ts.Ticks;
            return new DateTime(ticks * ts.Ticks);
        }
		
    }
}