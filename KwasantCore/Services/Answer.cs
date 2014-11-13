using Data.Entities;
using Data.Interfaces;
using KwasantCore.Interfaces;

namespace KwasantCore.Services
{
    public class Answer : IAnswer
    {
        public AnswerDO Update(IUnitOfWork uow, AnswerDO submittedAnswerDO)
        {
            AnswerDO answerDO = uow.AnswerRepository.GetOrCreateByKey(submittedAnswerDO.Id);
            answerDO.EventID = submittedAnswerDO.EventID;
            answerDO.AnswerStatus = submittedAnswerDO.AnswerStatus;
            answerDO.Text = submittedAnswerDO.Text;
            return answerDO;
        }
    }
}
    

