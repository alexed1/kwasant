using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
   public class DateUtility
    {
       public DateTimeOffset GenerateDateRange(string queryPeriod)
       {
           DateTimeOffset startDate = new DateTimeOffset();
           switch (queryPeriod.ToLower())
           {
               case "lastminutes":
                   startDate = DateTimeOffset.UtcNow.AddMinutes(-5);
                   break;
               case "lasthour":
                   startDate = DateTimeOffset.UtcNow.AddHours(-1);
                   break;
               case "lastday":
                   startDate = DateTimeOffset.UtcNow.AddDays(-1);
                   break;
               case "lastweek":
                   startDate = DateTimeOffset.UtcNow.AddDays(-7);
                   break;
           }
           return startDate;
       }
    }
}
