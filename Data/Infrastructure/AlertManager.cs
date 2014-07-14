//We rename .NET style "events" to "alerts" to avoid confusion with our business logic Alert concepts
using System;
using Data.Entities;
using Data.Interfaces;

namespace Data.Infrastructure
{
    //this class serves as both a registry of all of the defined alerts as well as a utility class.
    public static class AlertManager
    {       
        public delegate void CustomerCreatedHandler(IUnitOfWork uow, DateTime createdDate, UserDO userDO);
        public static event CustomerCreatedHandler AlertCustomerCreated;

        public delegate void BookingRequestCreatedHandler(IUnitOfWork uow, BookingRequestDO curBR);
        public static event BookingRequestCreatedHandler AlertBookingRequestCreated;

        #region Method
        
        /// <summary>
        /// Publish Customer Created event
        /// </summary>
        public static void CustomerCreated(IUnitOfWork uow, UserDO curUser)
        {
            if (AlertCustomerCreated != null)
                AlertCustomerCreated(uow, DateTime.Now, curUser);
        }

        public static void BookingRequestCreated(IUnitOfWork uow, BookingRequestDO curBR)
        {
            if (AlertBookingRequestCreated != null)
                AlertBookingRequestCreated(uow, curBR);
            
                
        }
   
        #endregion
    }
}