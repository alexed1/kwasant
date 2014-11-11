using System.Linq;
using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using KwasantCore.Interfaces;
using KwasantWeb.ViewModels;
using StructureMap;

namespace KwasantCore.Services
{


    public class Question : IQuestion
    {

      private IAnswer _answer;

        public Question()
        {
            _answer = ObjectFactory.GetInstance<IAnswer>();
        }


        public QuestionDO GetOrCreate(IUnitOfWork uow, int? curQuestionID)
        {
            QuestionDO curQuestionDO;
            if (curQuestionID == 0)
            {
                curQuestionDO = new QuestionDO();
                uow.QuestionsRepository.Add(curQuestionDO);
            }
            else
                curQuestionDO = uow.QuestionsRepository.GetByKey(curQuestionID);
            return curQuestionDO;
        }


        public void Update(IUnitOfWork uow, NegotiationQuestionVM submittedQuestion, NegotiationDO curNegotiationDO)
        {
            QuestionDO questionDO = GetOrCreate(uow, submittedQuestion.Id);

            questionDO = Mapper.Map<NegotiationQuestionVM, QuestionDO>(submittedQuestion);
            //VERIFY: these are properly automapped:
            //questionDO.AnswerType = submittedQuestion.Type;
            //questionDO.Text = submittedQuestion.Text;
            //questionDO.CalendarID = submittedQuestion.CalendarID;

            questionDO.Negotiation = curNegotiationDO;
            if (questionDO.QuestionStatus == 0)
                questionDO.QuestionStatus = QuestionState.Unanswered;
              
            RemoveDeletedAnswers(uow, questionDO, submittedQuestion);
            foreach (var submittedAnswer in submittedQuestion.Answers)
            {
                _answer.Update(uow, submittedAnswer);
            }
        }

        //Delete the existing answers which no longer exist in our proposed negotiation
        public void RemoveDeletedAnswers(IUnitOfWork uow, QuestionDO curQuestionDO, NegotiationQuestionVM submittedQuestion)
        {
            
            var proposedAnswerIDs = submittedQuestion.Answers.Select(a => a.Id);
            var existingAnswers = curQuestionDO.Answers.ToList();
            foreach (var existingAnswer in existingAnswers.Where(a => !proposedAnswerIDs.Contains(a.Id)))
            {
                uow.AnswerRepository.Remove(existingAnswer);
            }
        }
    }

    }
