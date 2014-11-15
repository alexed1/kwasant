using System;
using System.Collections.Generic;
using System.Diagnostics;
using Data.Entities;
using Data.Infrastructure;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.States;
using KwasantCore.Exceptions;
using KwasantCore.Managers.APIManagers.Packagers;
using KwasantCore.Interfaces;
using KwasantCore.Services;
using Newtonsoft.Json;
using StructureMap;
using Utilities;
using Utilities.Logging;

//NOTES: Do NOT put Incidents here. Put them in IncidentReporter


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
            AlertManager.AlertEmailReceived += ReportEmailReceived;
            AlertManager.AlertEventBooked += ReportEventBooked;
            AlertManager.AlertEmailSent += ReportEmailSent;
            AlertManager.AlertBookingRequestCreated += ReportBookingRequestCreated;
            AlertManager.AlertBookingRequestStateChange += ReportBookingRequestStateChanged;
            AlertManager.AlertExplicitCustomerCreated += ReportCustomerCreated;
        
            AlertManager.AlertUserRegistration += ReportUserRegistered;
            AlertManager.AlertBookingRequestCheckedOut += ReportBookingRequestCheckedOut;
            AlertManager.AlertBookingRequestOwnershipChange += ReportBookingRequestOwnershipChanged;
            AlertManager.AlertBookingRequestReserved += ReportBookingRequestReserved;
            AlertManager.AlertBookingRequestReservationTimeout += ReportBookingRequestReservationTimeOut;
            AlertManager.AlertStaleBookingRequestsDetected += ReportStaleBookingRequestsDetected;
  
            AlertManager.AlertPostResolutionNegotiationResponseReceived += OnPostResolutionNegotiationResponseReceived;
        }

        private void ReportStaleBookingRequestsDetected(BookingRequestDO[] oldBookingRequests)
        {
            string toNumber = ObjectFactory.GetInstance<IConfigRepository>().Get<string>("TwilioToNumber");
            var tw = ObjectFactory.GetInstance<ISMSPackager>();
            tw.SendSMS(toNumber, oldBookingRequests.Length + " Booking requests are over-due by 30 minutes.");
        }

        private void ReportBookingRequestReserved(int bookingRequestId, string bookerId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curBookingRequest = uow.BookingRequestRepository.GetByKey(bookingRequestId);
                if (curBookingRequest == null)
                    throw new EntityNotFoundException<BookingRequestDO>(bookingRequestId);
                var curBooker = uow.UserRepository.GetByKey(bookerId);
                if (curBooker == null)
                    throw new EntityNotFoundException<UserDO>(bookerId);

                if (!curBooker.Available.GetValueOrDefault())
                {
                    IConfigRepository configRepository = ObjectFactory.GetInstance<IConfigRepository>();
                    string fromAddress = configRepository.Get("EmailAddress_GeneralInfo");

                    const string subject = "A booking request has been reserved for you";
                    const string messageTemplate = "A booking request has been reserved for you ({0}). Click {1} to view the booking request.";

                    var bookingRequestURL = String.Format("{0}/BookingRequest/Details/{1}", Server.ServerUrl, curBookingRequest.Id);
                    var message = String.Format(messageTemplate, curBookingRequest.Subject, "<a href='" + bookingRequestURL + "'>here</a>");

                    var toRecipient = curBooker.EmailAddress;

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
            Logger.GetLogger().Info(string.Format("Reserved. BookingRequest ID : {0}, Booker ID: {1}", bookingRequestId, bookerId));
        }

        private void ReportBookingRequestReservationTimeOut(int bookingRequestId, string bookerId)
        {

            Logger.GetLogger().Info(string.Format("Reservation Timed out. BookingRequest ID : {0}, Booker ID: {1}", bookingRequestId, bookerId));
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

        private static void TrackablePropertyCreated(string name, string contextTable, int id, object status)
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

        private void ReportCustomerCreated(string curUserId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                FactDO curAction = new FactDO
                    {
                        Name = "",
                        PrimaryCategory = "User",
                        SecondaryCategory = "",
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

        public void ReportEmailReceived(int emailId, string customerId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                string emailSubject = uow.EmailRepository.GetByKey(emailId).Subject;
                emailSubject = emailSubject.Length <= 10 ? emailSubject : (emailSubject.Substring(0, 10) + "...");

                FactDO curAction = new FactDO()
                    {
                        Name = "",
                        PrimaryCategory = "Email",
                        SecondaryCategory = "",
                        Activity = "Received",
                        CustomerId = customerId,
                        CreateDate = DateTimeOffset.Now,
                        ObjectId = emailId
                    };
                curAction.Data = string.Format("{0} {1} {2}: ObjectId: {3} EmailAddress: {4} Subject: {5}", curAction.PrimaryCategory, curAction.SecondaryCategory, curAction.Activity, emailId, (uow.UserRepository.GetByKey(curAction.CustomerId).EmailAddress.Address), emailSubject);

                SaveFact(curAction);
            }
        }

        public void ReportEventBooked(int eventId, string customerId)
        {
            FactDO curAction = new FactDO()
                {
                    Name = "",
                    PrimaryCategory = "Event",
                    SecondaryCategory = "",
                    Activity = "Booked",
                    CustomerId = customerId,
                    CreateDate = DateTimeOffset.Now,
                    ObjectId = eventId
                };
            SaveFact(curAction);
        }
        public void ReportEmailSent(int emailId, string customerId)
        {
            FactDO curAction = new FactDO()
                {
                    Name = "",
                    PrimaryCategory = "Email",
                    SecondaryCategory = "",
                    Activity = "Sent",
                    CustomerId = customerId,
                    CreateDate = DateTimeOffset.Now,
                    ObjectId = emailId
                };
            SaveFact(curAction);
        }

        public void ReportBookingRequestCreated(int bookingRequestId) 
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var bookingRequestDO = uow.BookingRequestRepository.GetByKey(bookingRequestId);

                
                ObjectFactory.GetInstance<ITracker>().Track(bookingRequestDO.Customer, "BookingRequest", "Submit", new Dictionary<string, object> { { "BookingRequestId", bookingRequestDO.Id } });

                FactDO curAction = new FactDO()
                    {
                        Name = "",
                        PrimaryCategory = "BookingRequest",
                        SecondaryCategory = "",
                        Activity = "Created",
                        CustomerId = bookingRequestDO.CustomerID,
                        CreateDate = DateTimeOffset.Now,
                        ObjectId = bookingRequestId
                    };
                curAction.Data = curAction.Name + ": ID= " + curAction.ObjectId;
                AddFact(uow, curAction);
                uow.SaveChanges();
            }
        }
        public void ReportBookingRequestStateChanged(int bookingRequestId)
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
                        SecondaryCategory = "",
                        Activity = "StateChange",
                        CustomerId = bookingRequestDO.Customer.Id,
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


        public void ReportUserRegistered(UserDO curUser)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                FactDO curFactDO = new FactDO
                    {
                        Name = "",
                        PrimaryCategory = "User",
                        SecondaryCategory = "",
                        Activity = "Registered",
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

        public void ReportBookingRequestCheckedOut(int bookingRequestId, string bookerId)
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
                    CustomerId = bookingRequestDO.Customer.Id,
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

        //Do we need/use both this and the immediately preceding event? 
        public void ReportBookingRequestOwnershipChanged(int bookingRequestId, string bookerId)
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
                        CustomerId = bookingRequestDO.Customer.Id,
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