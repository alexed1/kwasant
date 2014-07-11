using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public static class DateUtility
    {
        public static DateRange GenerateDateRange(string queryPeriod)
        {
            DateRange dateRange = new DateRange();
            switch (queryPeriod.ToLower())
            {
                case "lastminutes":
                    dateRange.StartTime = DateTimeOffset.UtcNow.AddMinutes(-5);
                    break;
                case "lasthour":
                    dateRange.StartTime = DateTimeOffset.UtcNow.AddHours(-1);
                    break;
                case "lastday":
                    dateRange.StartTime = DateTimeOffset.UtcNow.AddDays(-1);
                    break;
                case "lastweek":
                    dateRange.StartTime = DateTimeOffset.UtcNow.AddDays(-7);
                    break;
            }
            dateRange.EndTime = DateTimeOffset.UtcNow;
            return dateRange;
        }
    }
   public struct DateRange
   {
       public DateTimeOffset StartTime { get; set; }
       public DateTimeOffset EndTime { get; set; }
   }
}
