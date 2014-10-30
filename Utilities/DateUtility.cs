using System;

namespace Utilities
{
    public static class DateUtility
    {
        public static DateRange GenerateDateRange(string queryPeriod)
        {
            DateRange dateRange = new DateRange();
            switch (queryPeriod.ToLower())
            {

                case "last5minutes":
                    dateRange.StartTime = DateTimeOffset.Now.AddMinutes(-5);
                    break;
                case "lasthour":
                    dateRange.StartTime = DateTimeOffset.Now.AddHours(-1);
                    break;
                case "lastday":
                    dateRange.StartTime = DateTimeOffset.Now.AddDays(-1);
                    break;
                case "lastweek":
                    dateRange.StartTime = DateTimeOffset.Now.AddDays(-7);
                    break;
            }
            dateRange.EndTime = DateTimeOffset.Now;
            return dateRange;
        }

        public static string TimeAgo(this DateTimeOffset dt)
        {
            TimeSpan span = DateTimeOffset.Now - dt;
            if (span.Days > 365)
            {
                int years = (span.Days/365);
                if (span.Days%365 != 0)
                    years += 1;
                return String.Format("about {0} {1} ago",
                    years, years == 1 ? "year" : "years");
            }
            if (span.Days > 30)
            {
                int months = (span.Days/30);
                if (span.Days%31 != 0)
                    months += 1;
                return String.Format("about {0} {1} ago",
                    months, months == 1 ? "month" : "months");
            }
            if (span.Days > 0)
                return String.Format("about {0} {1} ago",
                    span.Days, span.Days == 1 ? "day" : "days");
            if (span.Hours > 0)
                return String.Format("about {0} {1} ago",
                    span.Hours, span.Hours == 1 ? "hour" : "hours");
            if (span.Minutes > 0)
                return String.Format("about {0} {1} ago",
                    span.Minutes, span.Minutes == 1 ? "minute" : "minutes");
            if (span.Seconds > 5)
                return String.Format("about {0} seconds ago", span.Seconds);
            if (span.Seconds <= 5)
                return "just now";
            return string.Empty;
        }
    }

    public struct DateRange
    {
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
    }

}

