using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Data.Entities;


namespace Data.Interfaces
{
    public interface IAttendee
    {
        //this stuff looks like it belongs in an entity, not an interface//[Key]
        //int Id { get; set; }

      
        //EmailAddressDO EmailAddress { get; set; }
        //EventDO Event { get; set; }
        ////TO DO add status and type

        AttendeeDO Create(UserDO curUserDO);
        AttendeeDO Create(IUnitOfWork uow, string emailAddressString, EventDO curEventDO, String name = null);
        List<AttendeeDO> ConvertFromString(IUnitOfWork uow, string curAttendees);
        void ManageNegotiationAttendeeList(IUnitOfWork uow, NegotiationDO negotiationDO, List<String> attendees);
        List<AttendeeDO> ManageAttendeeList(IUnitOfWork uow, List<AttendeeDO> existingAttendeeSet, List<String> attendees);
        
    }

    public interface IAttendeeDO
    {
        [Key]
        int Id { get; set; }


        EmailAddressDO EmailAddress { get; set; }
        EventDO Event { get; set; }
        //TO DO add status and type

    }
    public class ParsedEmailAddress
    {
        public String Name { get; set; }
        public String Email { get; set; }

        public override string ToString()
        {
            if (String.IsNullOrEmpty(Name))
                return Email;
            return String.Format("<{0}>{1}", Name, Email); //is that the right order?
        }
    }
}