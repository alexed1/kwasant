//We rename .NET style "events" to "alerts" to avoid confusion with our business logic Alert concepts
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using Data.Entities;

namespace Data.Infrastructure
{
    //this class serves as both a registry of all of the defined alerts as well as a utility class.
    public static class AlertManager
    {
        //Alert handlers 
        #region Alerts

        public static event CustomerCreatedHandler alertCustomerCreated;
       
        #endregion

        // Delegates for Alert handlers.
        #region Delegate

        public delegate void CustomerCreatedHandler(KwasantSchedulingAlertData e);
      
        #endregion

        #region Method

        /// <summary>
        //Takes the alert string and constructs a proper argument for the event system 
        /// </summary>
        public static KwasantSchedulingAlertData Construct(string alertData)
        {
            KwasantSchedulingAlertData kwasantSchedulingAlertData = new KwasantSchedulingAlertData();
            List<string> alertDataPairs = alertData.Split(',').ToList();
            kwasantSchedulingAlertData.alertName = alertDataPairs[0].Split('=')[1].Trim();
            kwasantSchedulingAlertData.alertData = alertData.Substring(alertDataPairs[0].Length + 1);
            return kwasantSchedulingAlertData;
        }

        /// <summary>
        /// Publish Customer Created event
        /// </summary>
        public static void CustomerCreated(UserDO curUser)
        {
            string alertData = String.Format("name=CustomerCreated, createdate=" + DateTime.Now + ", UserId=" + curUser.Id);
            alertCustomerCreated(Construct(alertData));
        }

        
        
        #endregion
    }

    /// <summary>
    /// creates a custom version of EventArgs that allows for a single datastring
    /// without this, we can't pass any data with the events!
    /// The alertData string is itself composed of key/value pairs
    /// </summary>
    public class KwasantSchedulingAlertData : EventArgs
    {
       // public String alertData;
        private string _alertData;
        public String alertName;
        public String alertData
        {
            get { return _alertData; }
            set
            {
                _alertData = value;
                MakeHash(_alertData);
            }
        }

        public Dictionary<string, string> hash = new Dictionary<string, string>();

        //create a hash to easily access key value pairs inserted into event arguments
        //if the event args are not cleanly hashable, alertHash will stay empty.
        private void MakeHash(string curData)
        {
            
            List<string> kvpairs = curData.Split(',').ToList();
            foreach (var pair in kvpairs)
            {
                hash[pair.Split('=')[0]] = pair.Split('=')[1];
            }
            
        }
    }
}