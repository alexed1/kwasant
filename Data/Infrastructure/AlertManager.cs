﻿//We rename .NET style "events" to "alerts" to avoid confusion with our business logic Alert concepts
using System;
using Data.Entities;

namespace Data.Infrastructure
{
    //this class serves as both a registry of all of the defined alerts as well as a utility class.
    public static class AlertManager
    {       
        public delegate void CustomerCreatedHandler(DateTime createdDate, string userID);
        public static event CustomerCreatedHandler AlertCustomerCreated;

        #region Method
        
        /// <summary>
        /// Publish Customer Created event
        /// </summary>
        public static void CustomerCreated(UserDO curUser)
        {
            if (AlertCustomerCreated != null)
                AlertCustomerCreated(DateTime.Now, curUser.Id);
        }
   
        #endregion
    }
}