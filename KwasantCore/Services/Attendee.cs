using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using Data.Entities;
using Data.Entities.Enumerations;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Repositories;
using FluentValidation;
using KwasantICS.DDay.iCal;
using KwasantICS.DDay.iCal.DataTypes;
using KwasantICS.DDay.iCal.Serialization.iCalendar.Serializers;
using RazorEngine;
using StructureMap;
using Utilities;
using Encoding = System.Text.Encoding;

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
        public List<AttendeeDO> ConvertFromString(IUnitOfWork uow, string attendeeString)
        {
            List<AttendeeDO> curList = new List<AttendeeDO>();
            AttendeeDO curAttendeeDO;
            //split the string
            var attendees = attendeeString.Split(',').ToList();
            foreach (var attendee in attendees)
            {
                 //create an attendee
                //check the db. if we know about the email use that for the attendee
                //else create a new email address and use that.
                curAttendeeDO = new AttendeeDO                   
                {
                    EmailAddress = uow.EmailAddressRepository.GetOrCreateEmailAddress(attendee),
                    Name = attendee
                };
                //uow.AttendeeRepository.Add(curAttendeeDO); these don't have event ids yet, so let's not save them
                curList.Add(curAttendeeDO);
            }

            return curList;
        }
        
                


    }
}
