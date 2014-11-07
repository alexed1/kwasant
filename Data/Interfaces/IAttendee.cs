using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Data.Entities;


namespace Data.Interfaces
{
    public interface IAttendee
    {
        AttendeeDO Create(IUnitOfWork uow, string emailAddressString, EventDO curEventDO, String name = null);
        List<AttendeeDO> ConvertFromString(IUnitOfWork uow, string curAttendees);
        void ManageNegotiationAttendeeList(IUnitOfWork uow, NegotiationDO negotiationDO, List<String> attendees);
        List<AttendeeDO> ManageAttendeeList(IUnitOfWork uow, IList<AttendeeDO> existingAttendeeSet, List<String> attendees);
        IList<Int32?> GetRespondedAnswers(IUnitOfWork uow, List<Int32> answerIDs, string userID);
        AnswerDO GetSelectedAnswer(QuestionDO curQuestion, IEnumerable<Int32?> curUserAnswers);
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