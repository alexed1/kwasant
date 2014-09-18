//We rename .NET style "events" to "alerts" to avoid confusion with our business logic Alert concepts
using System;
using System.Diagnostics;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Microsoft.WindowsAzure;
using StructureMap;
using Utilities;
using Utilities.Logging;

namespace Data.Infrastructure
{
    //this class serves as both a registry of all of the defined alerts as well as a utility class.
    public static class AlertManager
    {       
        public delegate void CustomerCreatedHandler(string curUserId);
        public static event CustomerCreatedHandler AlertCustomerCreated;

        public delegate void BookingRequestCreatedHandler(int bookingRequestId);
        public static event BookingRequestCreatedHandler AlertBookingRequestCreated;

        public delegate void EmailReceivedHandler(int emailId, string customerId);
        public static event EmailReceivedHandler AlertEmailReceived;

        public delegate void EventBookedHandler(int eventId, string customerId);
        public static event EventBookedHandler AlertEventBooked;

        public delegate void EmailSentHandler(int emailId, string customerId);
        public static event EmailSentHandler AlertEmailSent;

        public delegate void EmailProcessingHandler(string dateReceived, string errorMessage);
        public static event EmailProcessingHandler AlertEmailProcessingFailure;

        public delegate void BookingRequestStateChangeHandler(int bookingRequestId);
        public static event BookingRequestStateChangeHandler AlertBookingRequestStateChange;

        public delegate void BookingRequestTimeoutStateChangeHandler(BookingRequestDO bookingRequestDO);
        public static event BookingRequestTimeoutStateChangeHandler AlertBookingRequestProcessingTimeout;

        public delegate void UserRegistrationHandler(UserDO curUser);
        public static event UserRegistrationHandler AlertUserRegistration;

        public delegate void BookingRequestCheckedOutHandler(int bookingRequestId, string bookerId);
        public static event BookingRequestCheckedOutHandler AlertBookingRequestCheckedOut;

        public delegate void BookingRequestOwnershipChangeHandler(int bookingRequestId, string bookerId);
        public static event BookingRequestOwnershipChangeHandler AlertBookingRequestOwnershipChange;

        public delegate void Error_EmailSendFailureHandler();
        public static event Error_EmailSendFailureHandler AlertError_EmailSendFailure;

        #region Method
        
        /// <summary>
        /// Publish Customer Created event
        /// </summary>
        public static void CustomerCreated(string curUserId)
        {
            if (AlertCustomerCreated != null)
                AlertCustomerCreated(curUserId);
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
        public static void BookingRequestProcessingTimeout(BookingRequestDO bookingRequestDO)
        {
            if (AlertBookingRequestProcessingTimeout != null)
                AlertBookingRequestProcessingTimeout(bookingRequestDO);
        }


        public static void UserRegistration(UserDO curUser)
        {
            if (AlertUserRegistration != null)
                AlertUserRegistration(curUser);
        }

        public static void BookingRequestCheckedOut(int bookingRequestId, string bookerId)
        {
            if (AlertBookingRequestStateChange != null)
                AlertBookingRequestCheckedOut(bookingRequestId, bookerId);
        }

        public static void BookingRequestOwnershipChange(int bookingRequestId, string bookerId)
        {
            if (AlertBookingRequestStateChange != null)
                AlertBookingRequestOwnershipChange(bookingRequestId, bookerId);
        }

        public static void Error_EmailSendFailure()
        {
            if (AlertError_EmailSendFailure != null)
                AlertError_EmailSendFailure();
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
            AlertManager.AlertCustomerCreated += NewCustomerCreated;
            AlertManager.AlertBookingRequestProcessingTimeout += ProcessTimeout;
            AlertManager.AlertUserRegistration += UserRegistration;
            AlertManager.AlertBookingRequestCheckedOut += ProcessBookingRequestCheckedOut;
            AlertManager.AlertBookingRequestOwnershipChange += BookingRequestOwnershipChange;
            AlertManager.AlertError_EmailSendFailure += Error_EmailSendFailure;
        }

        private void NewCustomerCreated(string curUserId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                FactDO curAction = new FactDO
                                       {
                                           Name = "CustomerCreated",
                                           PrimaryCategory = "User",
                                           SecondaryCategory = "Customer",
                                           Activity = "Created",
                                           CustomerId = curUserId,
                                           CreateDate = DateTimeOffset.Now,
                                           ObjectId = 0,
                                           Data = string.Format("User with email {0} created from: {1}", uow.UserRepository.GetByKey(curUserId).EmailAddress.Address, new StackTrace())
                                       };
                AddFact(uow, curAction);
                uow.SaveChanges();
            }
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
                FactDO curAction = new FactDO()
                                       {
                                           Name = "BookingRequest Created",
                                           PrimaryCategory = "Email",
                                           SecondaryCategory = "BookingRequest",
                                           Activity = "Created",
                                           CustomerId = uow.BookingRequestRepository.GetByKey(bookingRequestId).User.Id,
                                           CreateDate = DateTimeOffset.Now,
                                           ObjectId = bookingRequestId
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
            using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                AddFact(uow, curAction);
                uow.SaveChanges();
            }
        }
        private void AddFact(IUnitOfWork uow, FactDO curAction)
        {
            Debug.Assert(uow != null);
            Debug.Assert(curAction != null);
            var configRepo = ObjectFactory.GetInstance<IConfigRepository>();
            if (string.IsNullOrEmpty(curAction.Data))
            {
                curAction.Data = string.Format("{0} {1} {2}:" + " ObjectId: {3} CustomerId: {4}",
                    curAction.PrimaryCategory,
                    curAction.SecondaryCategory,
                    curAction.Activity,
                    curAction.ObjectId,
                    curAction.CustomerId);
            }
            if (configRepo.Get("LogLevel", String.Empty) == "Verbose")
                Logger.GetLogger().Info(curAction.Data);
            uow.FactRepository.Add(curAction);
        }
        public void ProcessTimeout(BookingRequestDO bookingRequestDO)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IncidentDO incidentDO = new IncidentDO();
                incidentDO.PrimaryCategory = "BookingRequest";
                incidentDO.SecondaryCategory = "Processing";
                incidentDO.CreateTime = DateTime.Now;
                incidentDO.Activity = "TimeOut";
                incidentDO.ObjectId = bookingRequestDO.Id;
                incidentDO.CustomerId = bookingRequestDO.User.Id;
                incidentDO.BookerId = bookingRequestDO.BookerId;
                uow.IncidentRepository.Add(incidentDO);
                uow.SaveChanges();
            }
        }


        public void UserRegistration(UserDO curUser)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                FactDO curFactDO = new FactDO
                {
                    Name = "UserRegistration Created",
                    PrimaryCategory = "User",
                    SecondaryCategory = "User",
                    Activity = "Created",
                    CustomerId = curUser.Id,
                    CreateDate = DateTimeOffset.Now,
                    ObjectId = 0,
                    Data = "User registrated with " + curUser.EmailAddress.Address
                };
                Logger.GetLogger().Info(curFactDO.Data);
                uow.FactRepository.Add(curFactDO);
                uow.SaveChanges();
            }

        }

        public void Error_EmailSendFailure()
        {
            using (var _uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IncidentDO incidentDO = new IncidentDO();
                incidentDO.PrimaryCategory = "Email";
                incidentDO.SecondaryCategory = "Send";
                incidentDO.CreateTime = DateTime.Now; ;
                incidentDO.Activity = "Failure";
                _uow.IncidentRepository.Add(incidentDO);
                _uow.SaveChanges();
            }
        }

        public void ProcessBookingRequestCheckedOut(int bookingRequestId, string bookerId)
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
                    SecondaryCategory = "Ownership",
                    Activity = "Checkout",
                    CustomerId = bookingRequestDO.User.Id,
                    ObjectId = bookingRequestDO.Id,
                    BookerId = bookerId,
                    Status = status,
                    CreateDate = DateTimeOffset.Now,
                };
                curAction.Data = "BookingRequest ID= " + bookingRequestDO.Id;
                AddFact(uow, curAction);
                uow.SaveChanges();
            }
        }

        public void BookingRequestOwnershipChange(int bookingRequestId, string bookerId)
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
                    SecondaryCategory = "Ownership",
                    Activity = "Change",
                    CustomerId = bookingRequestDO.User.Id,
                    ObjectId = bookingRequestDO.Id,
                    BookerId = bookerId,
                    Status = status,
                    CreateDate = DateTimeOffset.Now,
                };
                curAction.Data = "BookingRequest ID= " + bookingRequestDO.Id;
                AddFact(uow, curAction);
                uow.SaveChanges();

            }
        }
    }
}