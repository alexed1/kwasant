using System;
using System.Collections.Generic;
using System.Linq;
using KwasantCore.Services;
using StructureMap;


namespace Utilities
{
    public static class FilterUtility
    {
        private static readonly HashSet<String> _IgnoreEmails;
        static FilterUtility()
        {
            var configRepository = ObjectFactory.GetInstance<IConfigRepository>();
            _IgnoreEmails = new HashSet<String>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var reservedEmail in configRepository.Get("EmailAddress_KwasantReservedList", String.Empty).Split(','))
                _IgnoreEmails.Add(reservedEmail);

            _IgnoreEmails.Add(configRepository.Get("EmailAddress_GeneralInfo"));
            _IgnoreEmails.Add(configRepository.Get("INBOUND_EMAIL_USERNAME"));
        }
        
        public static bool IsReservedEmailAddress(String emailAddress)
        {
            return _IgnoreEmails.Contains(emailAddress);
        }

        public static IEnumerable<string> StripReservedEmailAddresses(IEnumerable<string> attendees, IConfigRepository configRepository)
        {
            return attendees.Where(a => !_IgnoreEmails.Contains(a));
        }

        public static string AddClickability(string originalData)
        {
            string clickableData = originalData;
            if (originalData != null)
            {
                try
                {
                    string objectType = originalData.Split(' ')[0].ToString();
                    string objectId = originalData.Split(':')[1].Substring(0, originalData.Split(':')[1].ToString().IndexOf(","));
                    string clickableLink = GetClickableLinks(objectType, objectId);

                    clickableData = clickableData.Replace(objectId, clickableLink);
                }
                catch { }
            }
            return clickableData;
        }

        private static string GetClickableLinks(string objectType,string objectId) 
        {
            Dictionary<string, string> dataUrlMappings = new Dictionary<string, string>();
            dataUrlMappings.Add("BookingRequest", "/Dashboard/Index/");
            dataUrlMappings.Add("Email", "/Dashboard/Index/");
            dataUrlMappings.Add("User", "/User/Details?userID=");

            if (objectType == "Email") 
            {
                //string bookingRequestId = new Email().FindEmailParentage(objectId);
                //if (bookingRequestId != null)
                //    return string.Format("<a target='_blank' href='{0}+{1}'>{2}</a>", dataUrlMappings[objectType], bookingRequestId, objectId);
            }
            if (objectType == "User")
            {
                //string userId = new Email().FindEmailParentage(objectId);
                //return string.Format("<a target='_blank' href='{0}+{1}'>{2}</a>", dataUrlMappings[objectType], userId, objectId);
            }
            return string.Format("<a target='_blank' href='{0}{1}'>{2}</a>", dataUrlMappings[objectType], objectId, objectId);
        }
    }
}
