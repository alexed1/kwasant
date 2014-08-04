using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Data.Entities;
using Data.Interfaces;
using StructureMap;

namespace KwasantCore.Services
{
    public class Attendee
    {
        public AttendeeDO Create (UserDO curUserDO)
        {
            AttendeeDO curAttendeeDO;
            curAttendeeDO = new AttendeeDO();
            curAttendeeDO.EmailAddress = curUserDO.EmailAddress;

            return curAttendeeDO;
        }

        public AttendeeDO Create(string emailAddressString, EventDO curEventDO, String name = null)
        {
            //create a new AttendeeDO
            //get or create the email address and associate it.
            AttendeeDO curAttendee = new AttendeeDO();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {        
                var emailAddressRepository = uow.EmailAddressRepository;
                EmailAddressDO emailAddress = emailAddressRepository.GetOrCreateEmailAddress(emailAddressString, name);
                curAttendee.EmailAddress = emailAddress;
                curAttendee.Event = curEventDO;  //do we have to also manually set the EventId? Seems unDRY
                //uow.AttendeeRepository.Add(curAttendee); //is this line necessary?
            
            }
            return curAttendee;
        }


        //the Event View Model returns attendees as a string. we'll want to do something more sophisticated involving typeahead and ajax but for now this is it
        //we want to convert that string into objects as quickly as possible once the data is on the server.
        public void ManageEventAttendeeList(IUnitOfWork uow, EventDO eventDO, string curAttendees)
        {
            List<AttendeeDO> existingAttendeeSet = eventDO.Attendees ?? new List<AttendeeDO>();
            List<AttendeeDO> newAttendees = ManageAttendeeList( uow, existingAttendeeSet,  curAttendees);
            foreach (var attendee in newAttendees)
            {
                attendee.Event = eventDO;
                attendee.EventID = eventDO.Id;
                uow.AttendeeRepository.Add(attendee);
            }         
        }

        public void ManageNegotiationAttendeeList(IUnitOfWork uow, NegotiationDO curNegDO, string curAttendees)
        {
            //List<AttendeeDO> existingAttendeeSet = curNegDO.Attendees ?? new List<AttendeeDO>();
            //List<AttendeeDO> newAttendees = ManageAttendeeList(uow, existingAttendeeSet, curAttendees);
            //foreach (var attendee in newAttendees)
            //{
            //    attendee.Negotiation = curNegDO;
            //    uow.AttendeeRepository.Add(attendee);
            //}
        }
        
        public List<AttendeeDO> ManageAttendeeList(IUnitOfWork uow, List<AttendeeDO> existingAttendeeSet, string curAttendees)
        {
            List<AttendeeDO> newAttendees = new List<AttendeeDO>();
            if (String.IsNullOrEmpty(curAttendees))
                curAttendees = String.Empty;

            var attendees = curAttendees.Split(',').ToList();


            var attendeesToDelete = existingAttendeeSet.Where(attendee => !attendees.Contains(attendee.EmailAddress.Address)).ToList();
            foreach (var attendeeToDelete in attendeesToDelete)
                uow.AttendeeRepository.Remove(attendeeToDelete);

            foreach (var attendee in attendees.Where(att => !existingAttendeeSet.Select(a => a.EmailAddress.Address).Contains(att)))
            {
                var newAttendee = new AttendeeDO
                {
                    EmailAddress = uow.EmailAddressRepository.GetOrCreateEmailAddress(attendee),            
                    Name = attendee
                };
                newAttendees.Add(newAttendee);
            }
            return newAttendees;
        }

        public void DetectEmailsFromBookingRequest(EventDO eventDO)
        {
            //Add the booking request user
            var curAttendeeDO = Create(eventDO.BookingRequest.User);
            eventDO.Attendees.Add(curAttendeeDO);

            var emailAddresses = GetEmailAddresses(eventDO.BookingRequest.HTMLText);
            emailAddresses.AddRange(GetEmailAddresses(eventDO.BookingRequest.PlainText));
            emailAddresses.AddRange(GetEmailAddresses(eventDO.BookingRequest.Subject));

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

            foreach(var email in uniqueEmails)
                eventDO.Attendees.Add(Create(email.Email, eventDO, email.Name));
        }

        public List<ParsedEmailAddress> GetEmailAddresses(String textToSearch)
        {
            if (String.IsNullOrEmpty(textToSearch))
                return new List<ParsedEmailAddress>();
            //This is the email regex.
            //It searches for emails in the format of <Some Person>somePerson@someDomain.someExtension

            //We assume that names can only contain letters, numbers, and spaces. We also allow for a blank name, in the form of <>
            const string nameRegex = @"[ a-zA-Z0-9]*";

            //We assume for now, that emails can only contain letters and numbers. This can be updated in the future (parsing emails is actually incredibly difficult).
            //See http://tools.ietf.org/html/rfc2822#section-3.4.1 in the future if we ever update this.
            const string emailUserNameRegex = @"[a-zA-Z0-9]*";

            //Domains can only contain letters or numbers.
            const string domainRegex = @"[a-zA-Z0-9]+";

            //Top level domain must be at least two characters long. Only allows letters, numbers or dashes.
            const string tldRegex = @"[a-zA-Z0-9\-]{2,}";

            //The name part is optional; we can find emails like 'rjrudman@gmail.com', or '<Robert Rudman>rjrudman@gmail.com'.
            //The regex uses named groups; 'name' and 'email'.
            //Name will contain the name, without <>. Email will contain the full email address (without the name).

            //Typically, you won't need to modify the below code, only the four variables defined above.
            var fullRegexExpression = String.Format(@"(<(?<name>{0})>)?(?<email>{1}@{2}\.{3})", nameRegex, emailUserNameRegex, domainRegex, tldRegex);

            var regex = new Regex(fullRegexExpression);

            var result = new List<ParsedEmailAddress>();
            foreach (Match match in regex.Matches(textToSearch))
            {
                var parse = new ParsedEmailAddress
                {
                    Name = match.Groups["name"].Value,
                    Email = match.Groups["email"].Value
                };
                result.Add(parse);
            }
            return result;
        }


        public class ParsedEmailAddress
        {
            public String Name { get; set; }
            public String Email { get; set; }

            public override string ToString()
            {
                if (String.IsNullOrEmpty(Name))
                    return Email;
                return String.Format("<{0}>{1}", Name, Email);
            }
        }
    }
}
