using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;
using Data.States;

namespace KwasantCore.Services
{
    public class Question
    {
        public void DeleteQuestion(IUnitOfWork uow, int questionId = 0)
        {
            CalendarDO calendarDO = uow.CalendarRepository.FindOne(c => c.QuestionId == questionId);
            if (calendarDO != null)
                uow.CalendarRepository.Remove(calendarDO);

            uow.QuestionsRepository.Remove(uow.QuestionsRepository.FindOne(q => q.Id == questionId));
            uow.SaveChanges();
        }

        public List<int> CreateQuestion(IUnitOfWork uow,int questionId, int negotiationId = 0)
        {
            List<int> quesId= new List<int>();
            quesId.Add(questionId);

            if (negotiationId > 0)
            {
                QuestionDO questionDO = new QuestionDO
                {
                    Negotiation = uow.NegotiationsRepository.FindOne(q => q.Id == negotiationId),
                    QuestionStatus = QuestionState.Unanswered,
                    Text = "Question",
                };

                uow.QuestionsRepository.Add(questionDO);
                uow.SaveChanges();
                quesId.Add(questionDO.Id);
            }
            else
                quesId.Add(0);

            return quesId;
        }
    }
}
