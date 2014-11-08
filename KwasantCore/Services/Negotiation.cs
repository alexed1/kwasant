using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.States;
using Utilities;

namespace KwasantCore.Services
{
    public class Negotiation : INegotiation
    {
        //get all answers
        public List<Int32> GetAnswerIDs(NegotiationDO curNegotiationDO)
        {
            return curNegotiationDO.Questions.SelectMany(q => q.Answers.Select(a => a.Id)).ToList();
        }

        //get answers for a particular user
        public IList<Int32?> GetAnswerIDsByUser(NegotiationDO curNegotiationDO, UserDO curUserDO, IUnitOfWork uow)
        {
            var _attendee = new Attendee(new EmailAddress(new ConfigRepository()));
            var answerIDs = GetAnswerIDs(curNegotiationDO);
            return  _attendee.GetRespondedAnswers(uow, answerIDs, curUserDO.Id);
        }

        public void CreateQuasiEmailForBookingRequest(IUnitOfWork uow, NegotiationDO curNegotiationDO, UserDO curUserDO, Dictionary<QuestionDO, AnswerDO> currentAnswers)
        {
            var isUpdateToAnswer = uow.NegotiationAnswerEmailRepository.GetQuery().Any(nae => nae.NegotiationID == curNegotiationDO.Id && nae.UserID == curUserDO.Id);

            var newLink = new NegotiationAnswerEmailDO();
            EmailDO quasiEmail = new EmailDO();
            newLink.Email = quasiEmail;
            newLink.User = curUserDO;
            newLink.Negotiation = curNegotiationDO;
            
            //Now we update it..
            const string bodyTextFormat = @"To Question: ""{0}"", answered ""{1}""";

            var bodyTextFull = new StringBuilder();
            if (isUpdateToAnswer)
                bodyTextFull.Append("*** User updated answers ***<br/>");

            //We grab each answer & question pair, and join with two line breaks.. IE:
            /*
             * To Question: "When should we meet", answered "7pm"
             * To Question: "Where should we meet", answered "Hard Rock Cafe"
             */

            bodyTextFull.Append(String.Join("<br/>", currentAnswers.Select(kvp => String.Format(bodyTextFormat, kvp.Key.Text, kvp.Value.Text))));

            quasiEmail.FromID = curUserDO.EmailAddressID;
            quasiEmail.DateReceived = DateTimeOffset.Now;
            
            var renderedText = bodyTextFull.ToString();
            quasiEmail.HTMLText = renderedText;
            quasiEmail.PlainText = renderedText.Replace("<br/>", Environment.NewLine);
            quasiEmail.Subject = "Response to Negotiation \"" + curNegotiationDO.Name + "\"";
            quasiEmail.ConversationId = curNegotiationDO.BookingRequestID;
            quasiEmail.EmailStatus = EmailState.Processed; //This email won't be sent

// ReSharper disable once PossibleInvalidOperationException -- Turn off resharper warning, BookingRequestID is guaranteed to be non-null (enforced by EF attribute)
            AlertManager.ConversationMemberAdded(curNegotiationDO.BookingRequestID.Value);

            uow.EmailRepository.Add(quasiEmail);
            uow.NegotiationAnswerEmailRepository.Add(newLink);
        }
    }
}
