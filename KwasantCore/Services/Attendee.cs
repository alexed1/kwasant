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

        public AttendeeDO Create(string emailAddressString, EventDO curEventDO)
        {
            //create a new AttendeeDO
            //get or create the email address and associate it.
            AttendeeDO curAttendee = new AttendeeDO();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {        
                var emailAddressRepository = uow.EmailAddressRepository;                
                EmailAddressDO emailAddress = emailAddressRepository.GetOrCreateEmailAddress(emailAddressString);
                curAttendee.EmailAddress = emailAddress;
                curAttendee.Event = curEventDO;  //do we have to also manually set the EventId? Seems unDRY
                //uow.AttendeeRepository.Add(curAttendee); //is this line necessary?
            
            }
            return curAttendee;
        }

        //the Event View Model returns attendees as a string. we'll want to do something more sophisticated involving typeahead and ajax but for now this is it
        //we want to convert that string into objects as quickly as possible once the data is on the server.
        public void ManageAttendeeList(IUnitOfWork uow, EventDO eventDO, string curAttendees)
        {
            var attendees = curAttendees.Split(',').ToList();

            var eventAttendees = eventDO.Attendees ?? new List<AttendeeDO>();
            var attendeesToDelete = eventAttendees.Where(attendee => !attendees.Contains(attendee.EmailAddress.Address)).ToList();
            foreach (var attendeeToDelete in attendeesToDelete)
                uow.AttendeeRepository.Remove(attendeeToDelete);

            foreach (var attendee in attendees.Where(att => !eventAttendees.Select(a => a.EmailAddress.Address).Contains(att)))
            {
                var newAttendee = new AttendeeDO
                {
                    EmailAddress = uow.EmailAddressRepository.GetOrCreateEmailAddress(attendee),
                    Event = eventDO,
                    EventID = eventDO.Id,
                    Name = attendee
                };
                uow.AttendeeRepository.Add(newAttendee);
            }
        }

        public void DetectFromBookingRequest(BookingRequestDO bookingRequestDO)
        {
            
        }

        public List<ParsedEmailAddress> GetEmailAddresses(String textToSearch)
        {
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
