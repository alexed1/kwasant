//We rename .NET style "events" to "alerts" to avoid confusion with our business logic Alert concepts
using System;
using Data.Entities;
using Data.Interfaces;
using StructureMap;
using Utilities.Logging;

namespace Data.Infrastructure
{
    //this class serves as both a registry of all of the defined alerts as well as a utility class.
    public static class AlertManager
    {       
        public delegate void CustomerCreatedHandler(DateTime createdDate, UserDO userDO);
        public static event CustomerCreatedHandler AlertCustomerCreated;

        public delegate void BookingRequestCreatedHandler(BookingRequestDO curBR);
        public static event BookingRequestCreatedHandler AlertBookingRequestCreated;

        public delegate void EmailReceivedHandler(int emailId,string customerId);
        public static event EmailReceivedHandler AlertEmailReceived;

        public delegate void EventBookedHandler(int eventId, string customerId);
        public static event EventBookedHandler AlertEventBooked;

        public delegate void EmailSentHandler(int emailId, string customerId);
        public static event EmailSentHandler AlertEmailSent;

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

        public static void EmailReceived(int emailId, string customerId)
        {
            if (AlertEmailReceived != null)
                AlertEmailReceived(emailId, customerId);
        }
        public static void EventBooked(int eventId, string customerId)
        {
            if (AlertEventBooked != null)
                AlertEventBooked(eventId, customerId);
        }
        public static void EmailSent(int emailId, string customerId)
        {
            if (AlertEmailSent != null)
                AlertEmailSent(emailId, customerId);
        }
   
        #endregion
    }


    public class AlertReporter
    {
        //Register for interesting events
        public void SubscribeToAlerts()
        {
            AlertManager.AlertEmailReceived += NewEmailReceived;
            AlertManager.AlertEventBooked += NewEventBooked;
            AlertManager.AlertEmailSent += EmailDispatched;
        }
        public void NewEmailReceived(int emailId, string customerId)
        {
            FactDO curAction = new FactDO()
            {
                Name = "EmailReceived",
                PrimaryCategory = "Email",
                SecondaryCategory = "Intake",
                Activity = "Received",
                CustomerId = customerId,
                CreateDate = DateTimeOffset.Now,
                ObjectId = emailId
            };
            SaveFact(curAction);
        }
        public void NewEventBooked(int eventId, string customerId)
        {
            FactDO curAction = new FactDO()
            {
                Name = "EventBooked",
                PrimaryCategory = "Event",
                SecondaryCategory = "Booking",
                Activity = "Received",
                CustomerId = customerId,
                CreateDate = DateTimeOffset.Now,
                ObjectId = eventId
            };
            SaveFact(curAction);
        }
        public void EmailDispatched(int emailId, string customerId)
        {
            FactDO curAction = new FactDO()
            {
                Name = "EmailSent",
                PrimaryCategory = "Email",
                SecondaryCategory = "Outbound",
                Activity = "Sent",
                CustomerId = customerId,
                CreateDate = DateTimeOffset.Now,
                ObjectId = emailId
            };
            SaveFact(curAction);
        }
        private void SaveFact(FactDO curAction)
        {
            curAction.Data = curAction.PrimaryCategory + " " + curAction.SecondaryCategory + " " + curAction.Activity + ":" + " ObjectId: " + curAction.ObjectId + " CustomerId: " + curAction.CustomerId;
            Logger.GetLogger().Info(curAction.Data);
            IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>();
            uow.FactRepository.Add(curAction);
            uow.SaveChanges();
        }
    }
}