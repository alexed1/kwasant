//We rename .NET style "events" to "alerts" to avoid confusion with our business logic Alert concepts

using System;
using Data.Entities;

namespace Data.Infrastructure
{
    //this class serves as both a registry of all of the defined alerts as well as a utility class.
    public static class AlertManager
    {
        public delegate void TrackablePropertyUpdatedHandler(string name, string contextTable, int id, object status);
        public static event TrackablePropertyUpdatedHandler AlertTrackablePropertyUpdated;

        public delegate void TrackablePropertyCreatedHandler(string name, string contextTable, int id, object status);
        public static event TrackablePropertyCreatedHandler AlertTrackablePropertyCreated;

        public delegate void TrackablePropertyDeletedHandler(string name, string contextTable, int id, int parentId, object status);
        public static event TrackablePropertyDeletedHandler AlertTrackablePropertyDeleted;
        
        public delegate void NewBookingRequestForPreferredBookerHandler(String bookerID, int bookingRequestID);
        public static event NewBookingRequestForPreferredBookerHandler AlertNewBookingRequestForPreferredBooker;

        public delegate void ConversationMemberAddedHandler(int bookingRequestID);
        public static event ConversationMemberAddedHandler AlertConversationMemberAdded;
        
        public delegate void ExplicitCustomerCreatedHandler(string curUserId);
        public static event ExplicitCustomerCreatedHandler AlertExplicitCustomerCreated;

        public delegate void PostResolutionNegotiationResponseReceivedHandler(int negotiationId);
        public static event PostResolutionNegotiationResponseReceivedHandler AlertPostResolutionNegotiationResponseReceived;

        public delegate void CustomerCreatedHandler(UserDO user);
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

        public delegate void Error_EmailSendFailureHandler(int emailId, string message);
        public static event Error_EmailSendFailureHandler AlertError_EmailSendFailure;

        public delegate void ErrorSyncingCalendarHandler(RemoteCalendarLinkDO calendarLink);
        public static event ErrorSyncingCalendarHandler AlertErrorSyncingCalendar;

        #region Method

        public static void TrackablePropertyUpdated(string name, string contextTable, int id, object status)
        {
            if (AlertTrackablePropertyUpdated != null)
                AlertTrackablePropertyUpdated(name, contextTable, id, status);
        }

        public static void TrackablePropertyCreated(string name, string contextTable, int id, object status)
        {
            if (AlertTrackablePropertyCreated != null)
                AlertTrackablePropertyCreated(name, contextTable, id, status);
        }

        public static void TrackablePropertyDeleted(string name, string contextTable, int id, int parentID, object status)
        {
            if (AlertTrackablePropertyDeleted != null)
                AlertTrackablePropertyDeleted(name, contextTable, id, parentID, status);
        }

        public static void NewBookingRequestForPreferredBooker(String bookerID, int bookingRequestID)
        {
            if (AlertNewBookingRequestForPreferredBooker != null)
                AlertNewBookingRequestForPreferredBooker(bookerID, bookingRequestID);
        }

        public static void ConversationMemberAdded(int bookingRequestID)
        {
            if (AlertConversationMemberAdded != null)
                AlertConversationMemberAdded(bookingRequestID);
        }

        /// <summary>
        /// Publish Customer Created event
        /// </summary>
        public static void ExplicitCustomerCreated(string curUserId)
        {
            if (AlertExplicitCustomerCreated != null)
                AlertExplicitCustomerCreated(curUserId);
        }

        public static void PostResolutionNegotiationResponseReceived(int negotiationDO)
        {
            if (AlertPostResolutionNegotiationResponseReceived != null)
                AlertPostResolutionNegotiationResponseReceived(negotiationDO);
        }

        public static void CustomerCreated(UserDO user)
        {
            if (AlertCustomerCreated != null)
                AlertCustomerCreated(user);
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

        public static void Error_EmailSendFailure(int emailId, string message)
        {
            if (AlertError_EmailSendFailure != null)
                AlertError_EmailSendFailure(emailId, message);
        }

        public static void ErrorSyncingCalendar(RemoteCalendarLinkDO calendarLink)
        {
            var handler = AlertErrorSyncingCalendar;
            if (handler != null)
                handler(calendarLink);
        }

        #endregion
    }


    public class AlertReporter
    {
        //Register for interesting events
        public void SubscribeToAlerts()
        {
            AlertManager.AlertTrackablePropertyUpdated += TrackablePropertyUpdated;
            AlertManager.AlertTrackablePropertyCreated += TrackablePropertyCreated;
            AlertManager.AlertTrackablePropertyDeleted += TrackablePropertyDeleted;

            AlertManager.AlertEmailReceived += NewEmailReceived;
            AlertManager.AlertEventBooked += NewEventBooked;
            AlertManager.AlertEmailSent += EmailDispatched;
            AlertManager.AlertBookingRequestCreated += ProcessBookingRequestCreated;
            AlertManager.AlertBookingRequestStateChange += ProcessBookingRequestStateChange;
            AlertManager.AlertExplicitCustomerCreated += NewExplicitCustomerCreated;
            AlertManager.AlertBookingRequestProcessingTimeout += ProcessTimeout;
            AlertManager.AlertUserRegistration += UserRegistration;
            AlertManager.AlertBookingRequestCheckedOut += ProcessBookingRequestCheckedOut;
            AlertManager.AlertBookingRequestOwnershipChange += BookingRequestOwnershipChange;
            AlertManager.AlertError_EmailSendFailure += Error_EmailSendFailure;
            AlertManager.AlertErrorSyncingCalendar += ErrorSyncingCalendar;
            AlertManager.AlertPostResolutionNegotiationResponseReceived += OnPostResolutionNegotiationResponseReceived;
        }

        private static void TrackablePropertyUpdated(string name, string contextTable, int id,
            object status)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var newFactDO = new FactDO
                {
                    Name = name,
                    PrimaryCategory = contextTable,
                    SecondaryCategory = "Journaling",
                    Activity = "Update",
                    ObjectId = id,
                    CreatedByID = ObjectFactory.GetInstance<ISecurityServices>().GetCurrentUser(),
                    Status = JsonConvert.SerializeObject(status),
                    CreateDate = DateTime.Now
                };
                uow.FactRepository.Add(newFactDO);
                uow.SaveChanges();
            }
        }

        private static void TrackablePropertyCreated(string name, string contextTable, int id,
            object status)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var newFactDO = new FactDO
                {
                    Name = name,
                    PrimaryCategory = contextTable,
                    SecondaryCategory = "Journaling",
                    Activity = "Create",
                    ObjectId = id,
                    CreatedByID = ObjectFactory.GetInstance<ISecurityServices>().GetCurrentUser(),
                    Status = JsonConvert.SerializeObject(status),
                    CreateDate = DateTime.Now
                };
                uow.FactRepository.Add(newFactDO);
                uow.SaveChanges();
            }
        }

        private static void TrackablePropertyDeleted(string name, string contextTable, int id, int parentID, object status)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var newFactDO = new FactDO
                {
                    Name = name,
                    PrimaryCategory = contextTable,
                    SecondaryCategory = "Journaling",
                    Activity = "Delete",
                    ObjectId = id,
                    TaskId = parentID,
                    CreatedByID = ObjectFactory.GetInstance<ISecurityServices>().GetCurrentUser(),
                    Status = JsonConvert.SerializeObject(status),
                    CreateDate = DateTime.Now
                };
                uow.FactRepository.Add(newFactDO);
                uow.SaveChanges();
            }
        }

        private static void OnPostResolutionNegotiationResponseReceived(int negotiationId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var negotiationDO = uow.NegotiationsRepository.GetByKey(negotiationId);
                
                IConfigRepository configRepository = ObjectFactory.GetInstance<IConfigRepository>();
                string fromAddress = configRepository.Get("EmailAddress_GeneralInfo");

                const string subject = "New response to resolved negotiation request";
                const string messageTemplate = "A customer has submitted a new response to an already-resolved negotiation request ({0}). Click {1} to view the booking request.";

                var bookingRequestURL = String.Format("{0}/BookingRequest/Details/{1}", Server.ServerUrl, negotiationDO.BookingRequestID);
                var message = String.Format(messageTemplate, negotiationDO.Name, "<a href='" + bookingRequestURL + "'>here</a>");

                var toRecipient = negotiationDO.BookingRequest.Booker.EmailAddress;

                EmailDO curEmail = new EmailDO
                {
                    Subject = subject,
                    PlainText = message,
                    HTMLText = message,
                    From = uow.EmailAddressRepository.GetOrCreateEmailAddress(fromAddress),
                    Recipients = new List<RecipientDO>()
                    {
                        new RecipientDO
                        {
                            EmailAddress = toRecipient,
                            EmailParticipantType = EmailParticipantType.To
                        }
                    }
                };

                uow.EnvelopeRepository.ConfigurePlainEmail(curEmail);
                uow.SaveChanges();
            }
        }

        private void NewExplicitCustomerCreated(string curUserId)
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
                var bookingRequestDO = uow.BookingRequestRepository.GetByKey(bookingRequestId);
                FactDO curAction = new FactDO()
                                       {
                                           Name = "BookingRequest Created",
                                           PrimaryCategory = "Email",
                                           SecondaryCategory = "BookingRequest",
                                           Activity = "Created",
                                           CustomerId = bookingRequestDO.UserID,
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
                incidentDO.BookerId = bookingRequestDO.UserID;
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

        private void ErrorSyncingCalendar(RemoteCalendarLinkDO calendarLink)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IncidentDO incidentDO = new IncidentDO();
                incidentDO.PrimaryCategory = "Calendar";
                incidentDO.SecondaryCategory = "Sync";
                incidentDO.CreateTime = DateTime.Now;
                incidentDO.Activity = "Failure";
                incidentDO.ObjectId = calendarLink.Id;
                incidentDO.CustomerId = calendarLink.LocalCalendar.OwnerID;
                uow.IncidentRepository.Add(incidentDO);
                uow.SaveChanges();
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