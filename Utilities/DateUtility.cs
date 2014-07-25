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
    }
   public struct DateRange
   {
       public DateTimeOffset StartTime { get; set; }
       public DateTimeOffset EndTime { get; set; }
   }
}
