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


    public class BookingRequest :IBookingRequest
    {
        private IAttendee _attendee;
        private IEmailAddress _emailAddress;

        public BookingRequest()
        {
            _attendee = ObjectFactory.GetInstance<IAttendee>();
            _emailAddress = ObjectFactory.GetInstance<IEmailAddress>();
        }
        public void Process(IUnitOfWork uow, BookingRequestDO bookingRequest)
        {
            var user = new User();
            bookingRequest.State = BookingRequestState.Unstarted;
            UserDO curUser = user.GetOrCreateFromBR(uow, bookingRequest.From);
            bookingRequest.User = curUser;
            bookingRequest.Instructions = ProcessShortHand(uow, bookingRequest.HTMLText);

            foreach (var calendar in bookingRequest.User.Calendars)  //this is smelly. Calendars are associated with a User. Why do we need to manually add them to BookingREquest.Calendars when they're easy to access?
                bookingRequest.Calendars.Add(calendar);
        }

        public List<object> GetAllByUserId(IBookingRequestDORepository curBookingRequestRepository, int start, int length, string userid)
        {
            return curBookingRequestRepository.GetAll().Where(e => e.User.Id == userid).Skip(start).Take(length).Select(e =>
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
                            Console.WriteLine("SUB" + e.Subject);
                            Console.WriteLine("FROM" + e.From);
                            Console.WriteLine("DATE" + e.DateReceived);
                            Console.WriteLine("HTML TEXT" + e.HTMLText);

                            Debug.WriteLine(e.Subject);
                            Debug.WriteLine(e.From);
                            Debug.WriteLine(e.DateReceived);
                            Debug.WriteLine(e.HTMLText);
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
            string bookerId = bookingRequestDO.BookerId;
            bookingRequestDO.State = BookingRequestState.Unstarted;
            bookingRequestDO.BookerId = null;
            bookingRequestDO.User = bookingRequestDO.User;
            uow.SaveChanges();
            bookingRequestDO.BookerId = bookerId;

            AlertManager.BookingRequestProcessingTimeout(bookingRequestDO);
            Logger.GetLogger().Info("Process Timed out. BookingRequest ID :" + bookingRequestDO.Id);
            bookingRequestDO.BookerId = null;
            // Send mail to Booker
            UserDO userDO = new UserDO();
            userDO = uow.UserRepository.GetByKey(bookerId);
            EmailAddressDO emailAddressDO = new EmailAddressDO(userDO.EmailAddress.Address);
            Email email = new Email(uow);
            string message = "BookingRequest ID :" + bookingRequestDO.Id + " Timed Out";
            string subject = "BookingRequest Timeout";

          //  EmailDO emailDO = email.GenerateBookerMessage(emailAddressDO, message,"BookingRequest Timeout");
            string toRecipient = emailAddressDO.Address;
            IConfigRepository configRepository = ObjectFactory.GetInstance<IConfigRepository>();
            string fromAddress = configRepository.Get("EmailAddress_GeneralInfo");
            EmailDO curEmail = email.GenerateBasicMessage(emailAddressDO, subject, message, fromAddress, toRecipient);
            email.Send(curEmail);
            uow.SaveChanges();
        }



        public void ExtractEmailAddresses(IUnitOfWork uow, EventDO eventDO)
        {
            //Add the booking request user
            var curAttendeeDO = _attendee.Create(eventDO.BookingRequest.User);
            eventDO.Attendees.Add(curAttendeeDO);
            IEnumerable<ParsedEmailAddress> recips;



            var emailAddresses = _emailAddress.ExtractFromString(eventDO.BookingRequest.HTMLText);
            emailAddresses.AddRange(_emailAddress.ExtractFromString(eventDO.BookingRequest.PlainText));
            emailAddresses.AddRange(_emailAddress.ExtractFromString(eventDO.BookingRequest.Subject));

            //need to add the addresses of people cc'ed or on the To line of the BookingRequest
            recips = from recip in eventDO.BookingRequest.Recipients
                     select new ParsedEmailAddress { Name = recip.EmailAddress.Name, Email = recip.EmailAddress.Address };
            emailAddresses.AddRange(recips);
            //Explanation of the query before:
            //First, we group by email (case insensitive).
            //Then, for each group of emails, we want to pick the first one in the group with a name
            //If none of the entries for an email have a name, we pick the first one we find

            //Example, if we had:
            //Test@gmail.com
            //<Mr Sir>Test@gmail.com
            //Test@gmail.com
            //Rob@gmail.com
            //TestTwo@gmail.com
            //<TestTwo>TestTwo@gmail.com
            //<TestTwoTwo>TestTwo@gmail.com

            //We want to group it like this:
            //Test@gmail.com
            //  - Test@gmail.com
            //  - <Mr Sir>Test@gmail.com
            //  - Test@gmail.com
            //Rob@gmail.com
            //  - Rob@gmail.com
            //TestTwo@gmail.com
            //  - TestTwo@gmail.com
            //  - <TestTwo>TestTwo@gmail.com
            //  - <TestTwoTwo>TestTwo@gmail.com

            //Then we apply the select to find the name
            //This flattens the set to be this:
            // <Mr Sir>Test@gmail.com
            // Rob@gmail.com
            // <TestTwo>TestTwo@gmail.com

            //This ensures uniquness, and tries to find a name for each email provided

            var uniqueEmails = emailAddresses.GroupBy(ea => ea.Email.ToLower()).Select(g =>
            {
                var potentialFirst = g.FirstOrDefault(e => !String.IsNullOrEmpty(e.Name));
                if (potentialFirst == null)
                    potentialFirst = g.First();
                return potentialFirst;
            });

            //Convert back from the special ParsedEmailAddress type to our normal entity
            IEnumerable<EmailAddressDO> addressList;         
            addressList = uniqueEmails.Select(parsemail => new EmailAddressDO {Address = parsemail.Email, Name = parsemail.Name});

            //we don't want addresses like "kwa@sant.com" added as attendees
            addressList = _emailAddress.FilterOutDomains(addressList, "sant.com");

            foreach (var email in addressList)
            {         
            var curAttendee = _attendee.Create(uow, email.Address, eventDO, email.Name);
            eventDO.Attendees.Add(curAttendee);
            }
    }



    }

}
