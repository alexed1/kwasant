using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Utilities;

namespace KwasantCore.Services
{
    public class Negotiation : INegotiation
    {
        //get all answers
        public List<Int32> GetAnswers(NegotiationDO curNegotiationDO)
        {
            return curNegotiationDO.Questions.SelectMany(q => q.Answers.Select(a => a.Id)).ToList();
        }

        //get answers for a particular user
        public IList<Int32?> GetAnswersByUser(NegotiationDO curNegotiationDO, UserDO curUserDO, IUnitOfWork uow)
        {
            var _attendee = new Attendee(new EmailAddress(new ConfigRepository()));
            var answerIDs = GetAnswers(curNegotiationDO);
            return  _attendee.GetRespondedAnswers(uow, answerIDs, curUserDO.Id);
        }

    }
}
