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

        public static IEnumerable<string> StripReservedEmailAddresses(string reservedEmailAddress)
        {
            var lstReservedEmailAddress = new List<string>();

            if (!string.IsNullOrWhiteSpace(reservedEmailAddress))
            {
                foreach (var emailtoignore in reservedEmailAddress.Split(','))
                { 
                    lstReservedEmailAddress.Add(emailtoignore);
                }
            }
            return lstReservedEmailAddress;
        }

    }

   
}
