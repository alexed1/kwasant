using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Data.Constants;

namespace KwasantCore.Services
{
    public class Answer
    {
        public void DeleteAnswer(IUnitOfWork uow, int answerId = 0)
        {
            uow.AnswersRepository.Remove(uow.AnswersRepository.FindOne(q => q.Id == answerId));
            uow.SaveChanges();
        }

        public List<int> CreateAnswer(IUnitOfWork uow, int answerId, int questionId = 0, int bookingRequestId=0)
        {
            List<int> ansId = new List<int>();
            ansId.Add(answerId);

            if (questionId > 0)
            {
                    EmailDO emailDO = uow.EmailRepository.FindOne(el => el.Id == bookingRequestId);
                    UserDO userDO = uow.UserRepository.FindOne(ur => ur.EmailAddressID == emailDO.FromID);
                    AnswerDO answerDO = new AnswerDO
                    {
                        QuestionID = questionId,
                        AnswerStatusID = AnswerStatus.Unstarted,
                        User = userDO,
                    };

                    uow.AnswersRepository.Add(answerDO);
                    uow.SaveChanges();
                    ansId.Add(answerDO.Id);
            }
            else
                ansId.Add(0);
            return ansId;
        }
    }
}
