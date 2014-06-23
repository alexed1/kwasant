//We rename .NET style "events" to "alerts" to avoid confusion with our business logic Alert concepts
using System;
using Data.Entities;

namespace Data.Infrastructure
{
    //this class serves as both a registry of all of the defined alerts as well as a utility class.
    public static class AlertManager
    {       
        public delegate void CustomerCreatedHandler(DateTime createdDate, UserDO userDO);
        public static event CustomerCreatedHandler AlertCustomerCreated;

        public delegate void BookingRequestCreatedHandler(BookingRequestDO curBR);
        public static event BookingRequestCreatedHandler AlertBookingRequestCreated;

        #region Method
        
        /// <summary>
        /// Publish Customer Created event
        /// </summary>
        public static void CustomerCreated(UserDO curUser)
        {
            if (AlertCustomerCreated != null)
                AlertCustomerCreated(DateTime.Now, curUser);
        }

        public static void BookingRequestCreated(BookingRequestDO curBR)
        {
            if (AlertBookingRequestCreated != null)
                AlertBookingRequestCreated(curBR);
            
                
        }
   
        #endregion
    }
}