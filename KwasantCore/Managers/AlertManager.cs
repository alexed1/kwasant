using System;
using System.Collections.Generic;
using System.Linq;

//We rename .NET style "events" to "alerts" to avoid confusion with our business logic Alert concepts

namespace KwasantCore.Managers
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

        public delegate void CustomerCreatedHandler(BooqitAlertData e);
      
        #endregion

        #region Method

        /// <summary>
        //Parses the alert data and prepares alert argument by separating alert name and alert data pair
        /// </summary>
        public static BooqitAlertData Parse(string alertData)
        {
            BooqitAlertData booqitAlertData = new BooqitAlertData();
            List<string> alertDataPairs = alertData.Split(',').ToList();
            booqitAlertData.alertName = alertDataPairs[0].Split('=')[1].Trim();
            booqitAlertData.alertData = alertData.Substring(alertDataPairs[0].Length + 1);
            return booqitAlertData;
        }

        /// <summary>
        /// Publish Customer Created event
        /// </summary>
        public static void CustomerCreated(string alertData)
        {
            alertCustomerCreated(Parse(alertData));
        }

        
        
        #endregion
    }

    /// <summary>
    /// creates a custom version of EventArgs that allows for a single datastring
    /// without this, we can't pass any data with the events!
    /// The alertData string is itself composed of key/value pairs
    /// </summary>
    public class BooqitAlertData : EventArgs
    {
        public String alertData;
        public String alertName;
    }
}