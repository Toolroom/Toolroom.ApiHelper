using System;

namespace Toolroom.ApiHelper
{
    public static class TimeSpanExtensions
    {
        public static TimeSpan Floor(this TimeSpan ts, TimeSpan roundTo)
        {
            var ticks = ts.Ticks / roundTo.Ticks;
            return new TimeSpan(ticks * roundTo.Ticks);
        }

        public static TimeSpan Round(this TimeSpan ts, TimeSpan roundTo)
        {
            var ticks = (ts.Ticks + (roundTo.Ticks / 2) + 1) / roundTo.Ticks;
            return new TimeSpan(ticks * roundTo.Ticks);
        }

        public static TimeSpan Ceiling(this TimeSpan ts, TimeSpan roundTo)
        {
            var ticks = (ts.Ticks + roundTo.Ticks - 1) / roundTo.Ticks;
            return new TimeSpan(ticks * roundTo.Ticks);
        }
    }
}