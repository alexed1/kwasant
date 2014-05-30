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
using UtilitiesLib;
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
        
                


    }
}
