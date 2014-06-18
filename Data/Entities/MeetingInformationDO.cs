using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities
{
   public class MeetingInformationDO
    {
       public string message { get; set; }

       public MeetingInformationDO(string MeetingInfo)
       {
           message = MeetingInfo;
       }
    }
}
