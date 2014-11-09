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

        public static IEnumerable<string> StripReservedEmailAddresses(IEnumerable<string> attendees, IConfigRepository configRepository)
        {
            if (attendees == null)
                throw new ArgumentNullException("attendees");
            if (configRepository == null)
                throw new ArgumentNullException("configRepository");
            var emailsListCsv = configRepository.Get("EmailAddress_KwasantReservedList") ?? string.Empty;
            IEnumerable<string> lstInvalidAttendee = emailsListCsv.Split(',');
            // we actually don't need to check configured reserved email addresses format because they never get to the result list.
            // just leave it for configuration manager =)
            return attendees.Except(lstInvalidAttendee);
        }

    }

   
}
