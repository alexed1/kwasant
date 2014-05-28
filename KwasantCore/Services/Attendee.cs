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
        private readonly IUnitOfWork _uow;
        public Attendee()
        {
            IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>();
            _uow = uow; //clean this up finish de-static work
        }


        public  AttendeeDO Create (UserDO curUserDO)
        {
            AttendeeDO curAttendeeDO;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                curAttendeeDO = new AttendeeDO();
                curAttendeeDO.EmailAddress = curUserDO.EmailAddress;
               // if (curUserDO.PersonDO.EmailAddress == null)
                   // throw new ArgumentException("Tried to create an Attendee from a User that doesn't have an email address");
            }
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
