using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Data.Entities;
using Data.Interfaces;

namespace KwasantCore.Services
{
    public class Attendee : IAttendee
    {
        public AttendeeDO Create (UserDO curUserDO)
        {
            AttendeeDO curAttendeeDO;
            curAttendeeDO = new AttendeeDO();
            curAttendeeDO.EmailAddress = curUserDO.EmailAddress;

            return curAttendeeDO;
        }

        public AttendeeDO Create(IUnitOfWork uow, string emailAddressString, EventDO curEventDO, String name = null)
        {
            //create a new AttendeeDO
            //get or create the email address and associate it.
            AttendeeDO curAttendee = new AttendeeDO();
               
            var emailAddressRepository = uow.EmailAddressRepository;
            EmailAddressDO emailAddress = emailAddressRepository.GetOrCreateEmailAddress(emailAddressString, name);
            curAttendee.EmailAddressID = emailAddress.Id;
            curAttendee.EmailAddress = emailAddress;
            curAttendee.Name = emailAddress.Name;
            curAttendee.Event = curEventDO;  //do we have to also manually set the EventId? Seems unDRY
            //uow.AttendeeRepository.Add(curAttendee); //is this line necessary?
            
            return curAttendee;
        }

        public List<AttendeeDO> ConvertFromString(IUnitOfWork uow, string curAttendees)
        {
            if (uow == null)
                throw new ArgumentNullException("uow");
            if (String.IsNullOrEmpty(curAttendees))
                return new List<AttendeeDO>();

            var attendees = curAttendees.Split(',');
            return ConvertFromStringList(uow, attendees);
        }

        private List<AttendeeDO> ConvertFromStringList(IUnitOfWork uow, IList<String> curAttendees)
        {
            if (curAttendees == null)
                return new List<AttendeeDO>();
            return curAttendees
                .Select(attendee => new AttendeeDO
                                        {
                                            EmailAddress = uow.EmailAddressRepository.GetOrCreateEmailAddress(attendee),
                                            Name = attendee
                                        })
                .ToList();
        }

        //the Event View Model returns attendees as a string. we'll want to do something more sophisticated involving typeahead and ajax but for now this is it
        //we want to convert that string into objects as quickly as possible once the data is on the server.
/*
        public void ManageEventAttendeeList(IUnitOfWork uow, EventDO eventDO, string curAttendees)
        {
            List<AttendeeDO> existingAttendeeSet = eventDO.Attendees ?? new List<AttendeeDO>();
            if (String.IsNullOrEmpty(curAttendees))
                curAttendees = String.Empty;

            var attendees = curAttendees.Split(',').ToList();

            List<AttendeeDO> newAttendees = ManageAttendeeList(uow, existingAttendeeSet, attendees);
            foreach (var attendee in newAttendees)
            {
                attendee.Event = eventDO;
                attendee.EventID = eventDO.Id;
                uow.AttendeeRepository.Add(attendee);
            }         
        }
*/

        public void ManageNegotiationAttendeeList(IUnitOfWork uow, NegotiationDO negotiationDO, List<String> attendees)
        {
            List<AttendeeDO> existingAttendeeSet = negotiationDO.Attendees ?? new List<AttendeeDO>();
            
            List<AttendeeDO> newAttendees = ManageAttendeeList(uow, existingAttendeeSet, attendees);
            foreach (var attendee in newAttendees)
            {
                attendee.Negotiation = negotiationDO;
                attendee.NegotiationID = negotiationDO.Id;
                uow.AttendeeRepository.Add(attendee);
            }
        }

        public List<AttendeeDO> ManageAttendeeList(IUnitOfWork uow, List<AttendeeDO> existingAttendeeSet, List<String> attendees)
        {
            var attendeesToDelete = existingAttendeeSet.Where(attendee => !attendees.Contains(attendee.EmailAddress.Address)).ToList();
            foreach (var attendeeToDelete in attendeesToDelete)
                uow.AttendeeRepository.Remove(attendeeToDelete);

            var newAttendees = attendees
                .Where(att => !existingAttendeeSet.Select(a => a.EmailAddress.Address).Contains(att))
                .ToList();
            return ConvertFromStringList(uow, newAttendees);
        }

       
       

       
    }
}
