//We rename .NET style "events" to "alerts" to avoid confusion with our business logic Alert concepts
using System;
using Data.Entities;
using Data.Interfaces;
using Microsoft.WindowsAzure;
using StructureMap;
using Utilities.Logging;

namespace Data.Infrastructure
{
    //this class serves as both a registry of all of the defined alerts as well as a utility class.
    public static class AlertManager
    {       
        public delegate void CustomerCreatedHandler(IUnitOfWork uow, DateTime createdDate, UserDO userDO);
        public static event CustomerCreatedHandler AlertCustomerCreated;

        public delegate void BookingRequestCreatedHandler(int bookingRequestId);
        public static event BookingRequestCreatedHandler AlertBookingRequestCreated;

        public delegate void EmailReceivedHandler(int emailId, string customerId);
        public static event EmailReceivedHandler AlertEmailReceived;

        public delegate void EventBookedHandler(int eventId, string customerId);
        public static event EventBookedHandler AlertEventBooked;

        public delegate void EmailSentHandler(int emailId, string customerId);
        public static event EmailSentHandler AlertEmailSent;

        public delegate void IncidentCreatedHandler(string dateReceived, string errorMessage);
        public static event IncidentCreatedHandler AlertEmailProcessingFailure;

        public delegate void BookingRequestStateChangeHandler(int bookingRequestId);
        public static event BookingRequestStateChangeHandler AlertBookingRequestStateChange;

        #region Method
        
        /// <summary>
        /// Publish Customer Created event
        /// </summary>
        public static void CustomerCreated(IUnitOfWork uow, UserDO curUser)
        {
            if (AlertCustomerCreated != null)
                AlertCustomerCreated(uow, DateTime.Now, curUser);
        }

        public static void BookingRequestCreated(int bookingRequestId)
        {
            if (AlertBookingRequestCreated != null)
                AlertBookingRequestCreated(bookingRequestId);
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
                
        public static void EmailProcessingFailure(string dateReceived, string errorMessage)
        {
            if (AlertEmailProcessingFailure != null)
                AlertEmailProcessingFailure(dateReceived, errorMessage);
        }

        public static void BookingRequestStateChange(int bookingRequestId)
        {
            if (AlertBookingRequestStateChange != null)
                AlertBookingRequestStateChange(bookingRequestId);
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
            AlertManager.AlertBookingRequestCreated += ProcessBookingRequestCreated;
            AlertManager.AlertBookingRequestStateChange += ProcessBookingRequestStateChange;
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
        public void ProcessBookingRequestCreated(int bookingRequestId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curBR = uow.BookingRequestRepository.GetByKey(bookingRequestId);
                if (curBR == null)
                    throw new ArgumentException(string.Format("Cannot find a Booking Request by given id:{0}", bookingRequestId), "bookingRequestId");
                FactDO curAction = new FactDO()
                                       {
                                           Name = "BookingRequest Created",
                                           PrimaryCategory = "Email",
                                           SecondaryCategory = "BookingRequest",
                                           Activity = "Created",
                                           CustomerId = curBR.User.Id,
                                           CreateDate = DateTimeOffset.Now,
                                           ObjectId = curBR.Id
                                       };
                curAction.Data = curAction.Name + ": ID= " + curAction.ObjectId;
                AddFact(uow, curAction);
                uow.SaveChanges();
            }
        }
        public void ProcessBookingRequestStateChange(int bookingRequestId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var bookingRequestDO = uow.BookingRequestRepository.GetByKey(bookingRequestId);
                if (bookingRequestDO == null)
                    throw new ArgumentException(string.Format("Cannot find a Booking Request by given id:{0}", bookingRequestId), "bookingRequestId");
                string status = bookingRequestDO.BookingRequestStateTemplate.Name;
                FactDO curAction = new FactDO()
                {
                    PrimaryCategory = "BookingRequest",
                    SecondaryCategory = "None",
                    Activity = "StateChange",
                    CustomerId = bookingRequestDO.User.Id,
                    ObjectId = bookingRequestDO.Id,
                    Status = status,
                    CreateDate = DateTimeOffset.Now,
                };
                curAction.Data = "BookingRequest ID= " + bookingRequestDO.Id;
                AddFact(uow, curAction);
                uow.SaveChanges();
                
            }
        }
        private void SaveFact(FactDO curAction)
        {
            curAction.Data = string.Format("{0} {1} {2}:" + " ObjectId: {3} CustomerId: {4}", 
                curAction.PrimaryCategory, 
                curAction.SecondaryCategory, 
                curAction.Activity, 
                curAction.ObjectId, 
                curAction.CustomerId);
            using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                AddFact(uow, curAction);
                uow.SaveChanges();
            }
        }
        private void AddFact(IUnitOfWork uow, FactDO curAction)
        {
            if (CloudConfigurationManager.GetSetting("LogLevel") == "Verbose")
                Logger.GetLogger().Info(curAction.Data);
            uow.FactRepository.Add(curAction);
        }
    }
}