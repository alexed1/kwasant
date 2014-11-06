using System;
using System.Collections.Generic;
using System.Diagnostics;
using Data.Entities;
using Data.Infrastructure;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.States;
using KwasantCore.Services;
using Newtonsoft.Json;
using StructureMap;
using Utilities;
using Utilities.Logging;

namespace KwasantCore.Managers
{
    public class AlertReporter
    {
        //Register for interesting events
        public void SubscribeToAlerts()
        {
            AlertManager.AlertTrackablePropertyUpdated += TrackablePropertyUpdated;
            AlertManager.AlertTrackablePropertyCreated += TrackablePropertyCreated;
            AlertManager.AlertTrackablePropertyDeleted += TrackablePropertyDeleted;
            AlertManager.AlertConversationMatched += AlertManagerOnAlertConversationMatched;
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

        private void AlertManagerOnAlertConversationMatched(int emailID, string subject, int bookingRequestID)
        {
            const string logMessageFormat = "Inbound Email ID {0} with subject '{1}' was matched to BR ID {2}";
            var logMessage = String.Format(logMessageFormat, emailID, subject, bookingRequestID);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var incidentDO = new IncidentDO
                {
                    ObjectId = emailID,
                    PrimaryCategory = "BookingRequest",
                    SecondaryCategory = "Conversation",
                    Notes = logMessage
                };
                uow.IncidentRepository.Add(incidentDO);
                uow.SaveChanges();
            }

            Logger.GetLogger().Info(logMessage);
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
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                string emailSubject = uow.EmailRepository.GetByKey(emailId).Subject;
                emailSubject = emailSubject.Length <= 10 ? emailSubject : (emailSubject.Substring(0, 10) + "...");

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
                curAction.Data = string.Format("{0} {1} {2}: ObjectId: {3} EmailAddress: {4} Subject: {5}", curAction.PrimaryCategory, curAction.SecondaryCategory, curAction.Activity, emailId, (uow.UserRepository.GetByKey(curAction.CustomerId).EmailAddress.Address), emailSubject);

                SaveFact(curAction);
            }
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
                curAction.Data = string.Format("{0} {1} {2}:" + " ObjectId: {3} EmailAddress: {4}",
                                               curAction.PrimaryCategory,
                                               curAction.SecondaryCategory,
                                               curAction.Activity,
                                               curAction.ObjectId,
                                               uow.UserRepository.GetByKey(curAction.CustomerId).EmailAddress.Address);
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

        private void Error_EmailSendFailure(int emailId, string message)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IncidentDO incidentDO = new IncidentDO();
                incidentDO.PrimaryCategory = "Email";
                incidentDO.SecondaryCategory = "Send";
                incidentDO.CreateTime = DateTime.Now; ;
                incidentDO.Activity = "Failure";
                incidentDO.ObjectId = emailId;
                incidentDO.Notes = message;
                uow.IncidentRepository.Add(incidentDO);
                uow.SaveChanges();
            }
            Email _email = ObjectFactory.GetInstance<Email>();
            _email.SendAlertEmail("Alert! Kwasant Error Reported: EmailSendFailure",
                                  string.Format(
                                      "EmailID: {0}\r\n" +
                                      "Message: {1}",
                                      emailId, message));
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
                incidentDO.Notes = calendarLink.LastSynchronizationResult;
                uow.IncidentRepository.Add(incidentDO);
                uow.SaveChanges();
            }

            Email email = ObjectFactory.GetInstance<Email>();
            email.SendAlertEmail("CalendarSync failure",
                                 string.Format(
                                     "CalendarSync failure for calendar link #{0} ({1}):\r\n" +
                                     "Customer id: {2},\r\n" +
                                     "Local calendar id: {3}\r\n," +
                                     "Remote calendar url: {4}",
                                     calendarLink.Id,
                                     calendarLink.LastSynchronizationResult,
                                     calendarLink.LocalCalendar.OwnerID,
                                     calendarLink.LocalCalendarID,
                                     calendarLink.RemoteCalendarHref));
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
                
                curAction.Data = string.Format("BookingRequest ID {0} Booker EmailAddress: {1}", bookingRequestDO.Id, uow.UserRepository.GetByKey(bookerId).EmailAddress.Address);
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
                
                curAction.Data = string.Format("BookingRequest ID {0} Booker EmailAddress: {1}", bookingRequestDO.Id, uow.UserRepository.GetByKey(bookerId).EmailAddress.Address);
                AddFact(uow, curAction);
                uow.SaveChanges();

            }
        }
    }
}