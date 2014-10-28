using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Utilities
{
    public static class FilterUtility
    {
        public static bool IsTestAttendee(string emailAddress) 
        {
            List<string> lstInvalidAttendee = new List<string>() { "kwa@sant.com", "scheduling@kwasant.com" };
            if (lstInvalidAttendee.Contains(emailAddress))
            {
                return true;
            }
            else { return false; }
        }

        public static IEnumerable<string> StripReservedEmailAddresses(List<string> attendees)
        {
            List<string> lstInvalidAttendee = new List<string>() { "hq@kwasant.com", "kwa@sant.com" };
            var lstReservedEmailAddress = new List<string>();

            foreach (var attendeeEmailAddress in attendees)
            {
                if (!lstInvalidAttendee.Contains(attendeeEmailAddress))
                {
                    lstReservedEmailAddress.Add(attendeeEmailAddress);
                }
            }
            return lstReservedEmailAddress;
        }

    }

   
}
