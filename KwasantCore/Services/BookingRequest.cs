using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Repositories;
using Data.States;
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

        public void Process(IUnitOfWork uow, BookingRequestDO bookingRequest)
        {
            bookingRequest.State = BookingRequestState.Unstarted;
            UserDO curUser = uow.UserRepository.GetOrCreateUser(bookingRequest.From);
            bookingRequest.User = curUser;
            bookingRequest.UserID = curUser.Id;
            bookingRequest.Instructions = ProcessShortHand(uow, bookingRequest.HTMLText);

            foreach (var calendar in bookingRequest.User.Calendars)
                //this is smelly. Calendars are associated with a User. Why do we need to manually add them to BookingREquest.Calendars when they're easy to access?
                bookingRequest.Calendars.Add(calendar);
        }

        public List<object> GetAllByUserId(IBookingRequestDORepository curBookingRequestRepository, int start,
            int length, string userid)
        {
            return
                curBookingRequestRepository.GetAll()
                    .Where(e => e.User.Id == userid)
                    .Skip(start)
                    .Take(length)
                    .Select(e =>
                        (object)new
                        {
                            id = e.Id,
                            subject = e.Subject,
                            dateReceived = e.DateReceived.ToString("M-d-yy hh:mm tt"),
                            linkedcalendarids = String.Join(",", e.User.Calendars.Select(c => c.Id))
                        }).ToList();
        }

        public int GetBookingRequestsCount(IBookingRequestDORepository curBookingRequestRepository, string userid)
        {
            return curBookingRequestRepository.GetAll().Where(e => e.User.Id == userid).Count();
        }

        public string GetUserId(IBookingRequestDORepository curBookingRequestRepository, int bookingRequestId)
        {
            return (from requests in curBookingRequestRepository.GetAll()
                    where requests.Id == bookingRequestId
                    select requests.User.Id).FirstOrDefault();
        }

        public object GetUnprocessed(IUnitOfWork uow)
        {
            return
                uow.BookingRequestRepository.GetAll()
                    .Where(e => e.State == BookingRequestState.Unstarted)
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
            bookingRequestDO.User = bookingRequestDO.User;
            uow.SaveChanges();
            bookingRequestDO.BookerID = bookerId;

            AlertManager.BookingRequestProcessingTimeout(bookingRequestDO);
            Logger.GetLogger().Info("Process Timed out. BookingRequest ID :" + bookingRequestDO.Id);
            bookingRequestDO.BookerID = null;
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

        public void ExtractEmailAddresses(IUnitOfWork uow, EventDO eventDO)
        {
            //Add the booking request user
            var curAttendeeDO = _attendee.Create(uow, eventDO.BookingRequest.User.EmailAddress.Address,eventDO, eventDO.BookingRequest.User.FirstName);
            eventDO.Attendees.Add(curAttendeeDO);
            var emailAddresses = _emailAddress.GetEmailAddresses(uow, eventDO.BookingRequest.HTMLText, eventDO.BookingRequest.PlainText, eventDO.BookingRequest.Subject);

            //need to add the addresses of people cc'ed or on the To line of the BookingRequest
            emailAddresses.AddRange(eventDO.BookingRequest.Recipients.Select(r => r.EmailAddress));

            foreach (var email in emailAddresses)
            {
                if (!FilterUtility.IsTestAttendee(email.Address))
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
                        .Where(u => bookerIDs.Contains(u.Id) && u.Available.Value)
                        .OrderBy(u => u.BookerBookingRequests.Count(br => br.State == BookingRequestState.Booking)).ToList();

                
                return preferredBookers.FirstOrDefault();
            }
        }
    }
}
