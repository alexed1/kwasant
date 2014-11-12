using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Repositories;
using Data.States;
using KwasantCore.Exceptions;
using KwasantCore.Interfaces;
using StructureMap;
using Utilities;
using Utilities.Logging;

namespace KwasantCore.Services
{


    public class BookingRequest : IBookingRequest
    {
        private IAttendee _attendee;
        private IEmailAddress _emailAddress;
        private readonly Email _email;

        public BookingRequest()
        {
            _attendee = ObjectFactory.GetInstance<IAttendee>();
            _email = ObjectFactory.GetInstance<Email>();
            _emailAddress = ObjectFactory.GetInstance<IEmailAddress>();
        }

        public static void ProcessNewBR(MailMessage message)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(uow.BookingRequestRepository, message);

                var newBookingRequest = new BookingRequest();
                newBookingRequest.Process(uow, bookingRequest);
                Email.FixInlineImages(bookingRequest);
                uow.SaveChanges();

                AlertManager.EmailReceived(bookingRequest.Id, bookingRequest.Customer.Id);
            }
        }

        public void Process(IUnitOfWork uow, BookingRequestDO bookingRequest)
        {
            bookingRequest.State = BookingRequestState.Unstarted;
            UserDO curUser = uow.UserRepository.GetOrCreateUser(bookingRequest.From);
            bookingRequest.Customer = curUser;
            bookingRequest.CustomerID = curUser.Id;
            bookingRequest.Instructions = ProcessShortHand(uow, bookingRequest.HTMLText);

            foreach (var calendar in bookingRequest.Customer.Calendars)
                //this is smelly. Calendars are associated with a Customer. Why do we need to manually add them to BookingREquest.Calendars when they're easy to access?
                bookingRequest.Calendars.Add(calendar);
        }

        public List<object> GetAllByUserId(IBookingRequestDORepository curBookingRequestRepository, int start,
            int length, string userid)
        {
            return
                curBookingRequestRepository.GetAll()
                    .Where(e => e.Customer.Id == userid)
                    .Skip(start)
                    .Take(length)
                    .Select(e =>
                        (object)new
                        {
                            id = e.Id,
                            subject = e.Subject,
                            dateReceived = e.DateReceived.ToString("M-d-yy hh:mm tt"),
                            linkedcalendarids = String.Join(",", e.Customer.Calendars.Select(c => c.Id))
                        }).ToList();
        }

        public int GetBookingRequestsCount(IBookingRequestDORepository curBookingRequestRepository, string userid)
        {
            return curBookingRequestRepository.GetAll().Where(e => e.Customer.Id == userid).Count();
        }

        public string GetUserId(IBookingRequestDORepository curBookingRequestRepository, int bookingRequestId)
        {
            return (from requests in curBookingRequestRepository.GetAll()
                    where requests.Id == bookingRequestId
                    select requests.Customer.Id).FirstOrDefault();
        }

        public object GetUnprocessed(IUnitOfWork uow)
        {
            return
                uow.BookingRequestRepository.GetAll()
                    .Where(e => (e.State == BookingRequestState.Unstarted) || (e.State == BookingRequestState.NeedsBooking))
                    .OrderByDescending(e => e.DateReceived)
                    .Select(
                        e =>
                        {
                            var text = e.PlainText ?? e.HTMLText;
                            if (String.IsNullOrEmpty(text))
                                text = String.Empty;
                            text = text.Trim();
                            if (text.Length > 400)
                                text = text.Substring(400);

                            return new
                            {
                                id = e.Id,
                                subject = e.Subject,
                                fromAddress = e.From.Address,
                                dateReceived = e.DateReceived.ToString("M-d-yy hh:mm tt"),
                                body = text
                            };
                        })
                    .ToList();
        }

        private List<InstructionDO> ProcessShortHand(IUnitOfWork uow, string emailBody)
        {
            List<int?> instructionIDs = ProcessTravelTime(emailBody).Select(travelTime => (int?)travelTime).ToList();
            instructionIDs.Add(ProcessAllDay(emailBody));
            instructionIDs = instructionIDs.Where(i => i.HasValue).Distinct().ToList();
            InstructionRepository instructionRepo = uow.InstructionRepository;
            return instructionRepo.GetQuery().Where(i => instructionIDs.Contains(i.Id)).ToList();
        }

        private IEnumerable<int> ProcessTravelTime(string emailBody)
        {
            const string regex = "{0}CC|CC{0}";

            Dictionary<int, int> travelTimeMapping = new Dictionary<int, int>
            {
                {30, InstructionConstants.TravelTime.Add30MinutesTravelTime},
                {60, InstructionConstants.TravelTime.Add60MinutesTravelTime},
                {90, InstructionConstants.TravelTime.Add90MinutesTravelTime},
                {120, InstructionConstants.TravelTime.Add120MinutesTravelTime}
            };

            List<int> instructions = new List<int>();

            //Matches cc[number] or [number]cc. Not case sensitive
            foreach (int allowedDuration in travelTimeMapping.Keys)
            {
                Regex reg = new Regex(String.Format(regex, allowedDuration), RegexOptions.IgnoreCase);
                if (!String.IsNullOrEmpty(emailBody))
                {
                    if (reg.IsMatch(emailBody))
                    {
                        instructions.Add(travelTimeMapping[allowedDuration]);
                    }
                }
            }
            return instructions;
        }


        private int? ProcessAllDay(string emailBody)
        {
            const string regex = "(ccADE)";

            Regex reg = new Regex(regex, RegexOptions.IgnoreCase);
            if (!String.IsNullOrEmpty(emailBody))
            {
                if (reg.IsMatch(emailBody))
                {
                    return InstructionConstants.EventDuration.MarkAsAllDayEvent;
                }
            }
            return null;
        }

        public IEnumerable<object> GetRelatedItems(IUnitOfWork uow, int bookingRequestId)
        {
            var events = uow.EventRepository
                .GetAll()
                .Where(e => e.BookingRequestID == bookingRequestId);
            //removed clarification requests, as there is no longer a direct connection. we'll need to collect them for this json via negotiation objects
            var invitationResponses = uow.InvitationResponseRepository
                .GetAll()
                .Where(e => e.Attendee != null && e.Attendee.Event != null &&
                            e.Attendee.Event.BookingRequestID == bookingRequestId);
            return Enumerable.Union<object>(events, invitationResponses);
        }

        public void Timeout(IUnitOfWork uow, BookingRequestDO bookingRequestDO)
        {
            string bookerId = bookingRequestDO.BookerID;
            bookingRequestDO.State = BookingRequestState.Unstarted;
            bookingRequestDO.BookerID = null;
            bookingRequestDO.Booker = null;
            bookingRequestDO.Customer = bookingRequestDO.Customer;
            bookingRequestDO.PreferredBookerID = null;
            bookingRequestDO.PreferredBooker = null;
            uow.SaveChanges();

            AlertManager.BookingRequestProcessingTimeout(bookingRequestDO.Id, bookerId);
            Logger.GetLogger().Info("Process Timed out. BookingRequest ID :" + bookingRequestDO.Id);
            // Send mail to Booker
            var curbooker = uow.UserRepository.GetByKey(bookerId);
            string message = "BookingRequest ID : " + bookingRequestDO.Id + " Timed Out <br/>Subject : " + bookingRequestDO.Subject;

            if (curbooker.EmailAddress != null)
                message += "<br/>Booker : " + curbooker.EmailAddress.Address;
            else
                message += "<br/>Booker : " + curbooker.FirstName;

            string subject = "BookingRequest Timeout";
            string toRecipient = curbooker.EmailAddress.Address;
            IConfigRepository configRepository = ObjectFactory.GetInstance<IConfigRepository>();
            string fromAddress = configRepository.Get<string>("EmailAddress_GeneralInfo");
            EmailDO curEmail = _email.GenerateBasicMessage(uow, subject, message, fromAddress, toRecipient);
            uow.EnvelopeRepository.ConfigurePlainEmail(curEmail);
            uow.SaveChanges();
        }

        public IEnumerable<String> ExtractEmailAddresses(BookingRequestDO bookingRequestDO)
        {
            var emailAddress = new EmailAddress(ObjectFactory.GetInstance<IConfigRepository>());

            var allThreads = bookingRequestDO.ConversationMembers.Union(new[] {bookingRequestDO}).ToList();
            
            //Get the emails of every recipient in every email
            var emailThreads = allThreads.SelectMany(b => b.Recipients.Select(r => r.EmailAddress.Address).Union(new[] {b.From.Address}));
            
            //Get the emails found within email text
            var emailsInText = new List<String>();
            foreach (var thread in allThreads)
            {
                emailsInText.AddRange(emailAddress.ExtractFromString(thread.HTMLText, thread.PlainText, thread.Subject));
            }

            //Get the attendees of all events
            var eventAttendees = bookingRequestDO.Events.SelectMany(ev => ev.Attendees.Select(a => a.EmailAddress.Address));

            return emailThreads.Union(emailsInText).Union(eventAttendees).Where(e => !FilterUtility.IsReservedEmailAddress(e));
        }

        public void ExtractEmailAddresses(IUnitOfWork uow, EventDO eventDO)
        {
            //Add the booking request user
            var curAttendeeDO = _attendee.Create(uow, eventDO.BookingRequest.Customer.EmailAddress.Address,eventDO, eventDO.BookingRequest.Customer.FirstName);
            eventDO.Attendees.Add(curAttendeeDO);
            var emailAddresses = _emailAddress.GetEmailAddresses(uow, eventDO.BookingRequest.HTMLText, eventDO.BookingRequest.PlainText, eventDO.BookingRequest.Subject);

            //need to add the addresses of people cc'ed or on the To line of the BookingRequest
            emailAddresses.AddRange(eventDO.BookingRequest.Recipients.Select(r => r.EmailAddress));

            foreach (var email in emailAddresses)
            {
                if (!FilterUtility.IsReservedEmailAddress(email.Address))
                {
                    var curAttendee = _attendee.Create(uow, email.Address, eventDO, email.Name);
                    eventDO.Attendees.Add(curAttendee);
                }
            }
        }

        //if curBooker is null, will return all BR's of state "Booking"
        public object GetCheckOutBookingRequest(IUnitOfWork uow, string curBooker)
        {
            return
                uow.BookingRequestRepository.GetAll()
                .Where(e => e.State == BookingRequestState.Booking && ((!String.IsNullOrEmpty(curBooker)) ? e.BookerID == curBooker : true))
                    .OrderByDescending(e => e.DateReceived)
                    .Select(
                        e =>
                        {
                            return new
                            {
                                id = e.Id,
                                subject = e.Subject,
                                fromAddress = e.From.Address,
                                dateReceived = e.DateReceived.ToString("M-d-yy hh:mm tt"),
                                body =
                                    e.HTMLText.Trim().Length > 400
                                        ? e.HTMLText.Trim().Substring(0, 400)
                                        : e.HTMLText.Trim()
                            };
                        })
                    .ToList();
        }

        public string getCountDaysAgo(DateTimeOffset dateReceived)
        {
            string daysAgo = string.Empty;
            int countDays = (System.DateTime.Today - dateReceived).Days;
            if (countDays > 0)
                daysAgo = " (" + countDays + " days ago)";
            else
            {
                daysAgo = " (" + dateReceived.ToLocalTime().ToString("T") + ")";
            }

            return daysAgo;
        }

        public object GetAllBookingRequests(IUnitOfWork uow)
        {
            return
                uow.BookingRequestRepository.GetAll()
                    .OrderByDescending(e => e.DateReceived)
                    .Select(
                        e =>
                        {
                            return new
                            {
                                id = e.Id,
                                subject = e.Subject,
                                fromAddress = e.From.Address,
                                dateReceived = e.DateReceived.ToString("M-d-yy hh:mm tt"),
                                body =
                                    e.HTMLText.Trim().Length > 400
                                        ? e.HTMLText.Trim().Substring(0, 400)
                                        : e.HTMLText.Trim()
                            };
                        })
                    .ToList();
        }

        public List<BookingRequestDO> GetAwaitingResponse(IUnitOfWork uow, string currBooker)
        {
            return
                uow.BookingRequestRepository.GetAll()
                    .Where(e => (e.State == BookingRequestState.AwaitingClient) && currBooker == GetPreferredBooker(e).Id)
                    .OrderByDescending(e => e.DateReceived).ToList();
        }

        public UserDO GetPreferredBooker(BookingRequestDO bookingRequestDO)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var bookerRoleID = uow.AspNetUserRolesRepository.GetRoleID(Roles.Booker);

                var bookerIDs =
                    uow.AspNetUserRolesRepository.GetQuery()
                        .Where(ur => ur.RoleId == bookerRoleID)
                        .Select(ur => ur.UserId);

                var preferredBookers =
                    uow.UserRepository.GetQuery()
                        .Where(u => bookerIDs.Contains(u.Id) && u.Available == true)
                        .OrderBy(u => u.BookerBookingRequests.Count(br => br.State == BookingRequestState.Booking)).ToList();

                return preferredBookers.FirstOrDefault();
            }
        }

        public void AddExpectedResponseForBookingRequest(IUnitOfWork uow, EmailDO emailDO, int bookingRequestID)
        {
            //We don't wait for responses from CC or BCC recipients
            foreach (var recipient in emailDO.To)
            {
                var currExpectedResponse = new ExpectedResponseDO
                {
                    Status = ExpectedResponseStatus.Active,
                    User = uow.UserRepository.GetOrCreateUser(recipient.Address),
                    AssociatedObjectID = bookingRequestID,
                    AssociatedObjectType = "BookingRequest"
                };
                uow.ExpectedResponseRepository.Add(currExpectedResponse);
            }               
        }

        public void AddExpectedResponseForNegotiation(IUnitOfWork uow, EmailDO emailDO, int negotiationID)
        {
            //We don't wait for responses from CC or BCC recipients
            foreach (var recipient in emailDO.To)
            {
                var currExpectedResponse = new ExpectedResponseDO
                {
                    Status = ExpectedResponseStatus.Active,
                    User = uow.UserRepository.GetOrCreateUser(recipient.Address),
                    AssociatedObjectID = negotiationID,
                    AssociatedObjectType = "Negotiation"
                };
                uow.ExpectedResponseRepository.Add(currExpectedResponse);
            }
        }

        public void AcknowledgeResponseToBookingRequest(IUnitOfWork uow, int bookingRequestID, String userID)
        {
            var bookingRequestDO = uow.BookingRequestRepository.GetByKey(bookingRequestID);
            //Now we mark expected responses as complete

            var negotiationIDs = bookingRequestDO.Negotiations.Select(n => n.Id);

            var expectedResponses = uow.ExpectedResponseRepository.GetQuery()
                .Where(
                er =>
                    er.UserID == userID &&
                    er.Status == ExpectedResponseStatus.Active &&
                    er.AssociatedObjectID == bookingRequestID &&
                    er.AssociatedObjectType == "BookingRequest")
                .Union(uow.ExpectedResponseRepository.GetQuery()
                .Where(
                er => 
                    er.UserID == userID &&
                    er.Status == ExpectedResponseStatus.Active &&
                    negotiationIDs.Contains(er.AssociatedObjectID) &&
                    er.AssociatedObjectType == "Negotiation"
                ));
            

            foreach (var expectedResponse in expectedResponses)
                expectedResponse.Status = ExpectedResponseStatus.ResponseReceived;

            if (expectedResponses.Any())
                AlertManager.ResponseReceived(bookingRequestDO.Id, bookingRequestDO.BookerID, userID);
        }

        public void AcknowledgeResponseToNegotiationRequest(IUnitOfWork uow, int negotiationID, String userID)
        {
            var negotiationDO = uow.NegotiationsRepository.GetByKey(negotiationID);
            //Now we mark expected responses as complete
            var expectedResponses = uow.ExpectedResponseRepository.GetQuery()
                .Where(
                er =>
                    er.UserID == userID &&
                    er.Status == ExpectedResponseStatus.Active &&
                    er.AssociatedObjectID == negotiationID &&
                    er.AssociatedObjectType == "Negotiation");

            foreach (var expectedResponse in expectedResponses)
                expectedResponse.Status = ExpectedResponseStatus.ResponseReceived;

            if (expectedResponses.Any())
                AlertManager.ResponseReceived(negotiationDO.BookingRequest.Id, negotiationDO.BookingRequest.BookerID, userID);
        }

        public String GetConversationThread(BookingRequestDO bookingRequestDO)
        {
            const string conversationThreadFormat = @"
<div style=""border: solid 1px #e7e7e7;background: #FFFFF;font-family:'Calibri';font-size: 17px;font-weight: 200;margin-bottom: 10px;display:block;line-height: 30px;"">

    <div style=""background: #dbdbdb;width: 100%;height: 30px;padding-top: 5px;padding-bottom: 5px;	overflow: hidden;"">
	    <span style=""float:left;width:60%;padding-left:15px;font-weight:bold;"">From: {0}</span>
	    <span style=""float:right;width:30%;padding-right:15px;text-align:right;"">{1}</span>
    </div>

    <div style=""padding:15px; color:rgb(51,51,51);"">
    {2}
    </div>
</div>
";
            var threads = bookingRequestDO.ConversationMembers.Union(new[] {bookingRequestDO});

            var result = String.Join("", threads.OrderByDescending(b => b.DateReceived).Select(e =>
                String.Format(conversationThreadFormat, e.From.Name,
                    e.DateReceived.TimeAgo(), e.HTMLText)));

            return result;
        }

        public void CheckOut(int bookingRequestId, string bookerId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var bookingRequestDO = uow.BookingRequestRepository.GetByKey(bookingRequestId);
                if (bookingRequestDO == null)
                    throw new EntityNotFoundException<BookingRequestDO>(bookingRequestId);
                var bookerDO = uow.UserRepository.GetByKey(bookerId);
                if (bookerDO == null)
                    throw new EntityNotFoundException<UserDO>(bookerId);
                bookingRequestDO.State = BookingRequestState.Booking;
                bookingRequestDO.BookerID = bookerId;
                bookingRequestDO.Booker = bookerDO;
                bookingRequestDO.PreferredBookerID = bookerId;
                bookingRequestDO.PreferredBooker = bookerDO;
                bookingRequestDO.LastUpdated = DateTimeOffset.Now;
                uow.SaveChanges();
                AlertManager.BookingRequestCheckedOut(bookingRequestDO.Id, bookerId);
            }
        }

        public void ReleaseBooker(int bookingRequestId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                BookingRequestDO bookingRequestDO = uow.BookingRequestRepository.GetByKey(bookingRequestId);
                if (bookingRequestDO == null)
                    throw new EntityNotFoundException<BookingRequestDO>(bookingRequestId);
                bookingRequestDO.State = BookingRequestState.Unstarted;
                bookingRequestDO.BookerID = null;
                bookingRequestDO.Booker = null;
                bookingRequestDO.PreferredBookerID = null;
                bookingRequestDO.PreferredBooker = null;
                bookingRequestDO.Customer = bookingRequestDO.Customer;
                uow.SaveChanges();
            }
        }

        public void Reactivate(IUnitOfWork uow, BookingRequestDO bookingRequestDO)
        {
            if (uow == null)
                throw new ArgumentNullException("uow");
            if (bookingRequestDO == null)
                throw new ArgumentNullException("bookingRequestDO");
            bookingRequestDO.State = BookingRequestState.NeedsBooking;
            if (bookingRequestDO.PreferredBooker != null)
            {
                Reserve(uow, bookingRequestDO, bookingRequestDO.PreferredBooker);
            }
        }

        public void Reserve(IUnitOfWork uow, BookingRequestDO bookingRequestDO, UserDO booker)
        {
            if (uow == null)
                throw new ArgumentNullException("uow");
            if (bookingRequestDO == null)
                throw new ArgumentNullException("bookingRequestDO");
            if (booker == null)
                throw new ArgumentNullException("booker");

            bookingRequestDO.Availability = BookingRequestAvailability.ReservedPB;
            uow.SaveChanges();

            AlertManager.BookingRequestReserved(bookingRequestDO.Id, booker.Id);
        }

        public void ReservationTimeout(IUnitOfWork uow, BookingRequestDO bookingRequestDO)
        {
            if (uow == null)
                throw new ArgumentNullException("uow");
            if (bookingRequestDO == null)
                throw new ArgumentNullException("bookingRequestDO");
            if (bookingRequestDO.State == BookingRequestState.NeedsBooking)
            {
                string bookerId = bookingRequestDO.PreferredBookerID;
                bookingRequestDO.PreferredBookerID = null;
                bookingRequestDO.PreferredBooker = null;
                bookingRequestDO.Availability = BookingRequestAvailability.Available;
                uow.SaveChanges();

                AlertManager.BookingRequestReservationTimeout(bookingRequestDO.Id, bookerId);
            }

        }
    }
}
