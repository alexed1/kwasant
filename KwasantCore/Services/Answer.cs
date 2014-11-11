using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using KwasantCore.Interfaces;
using KwasantWeb.ViewModels;

namespace KwasantCore.Services
{
    public class Answer : IAnswer
    {

        public AnswerDO GetOrCreate(IUnitOfWork uow, int? curAnswerID)
        {
            AnswerDO curAnswerDO;
            if (curAnswerID == 0)
            {
                curAnswerDO = new AnswerDO();
                uow.AnswerRepository.Add(curAnswerDO);
            }
            else
                curAnswerDO = uow.AnswerRepository.GetByKey(curAnswerID);
            return curAnswerDO;
        }

        public void Update(IUnitOfWork uow, NegotiationAnswerVM submittedAnswer)
        {
            AnswerDO answerDO = GetOrCreate(uow, submittedAnswer.Id);
            answerDO = Mapper.Map<NegotiationAnswerVM, AnswerDO>(submittedAnswer);
            //verify that these are all automapped properly:
            //answerDO.EventID = submittedAnswer.EventID;
            //answerDO.AnswerStatus = submittedAnswer.AnswerState;
            //answerDO.Question = questionDO;
            //answerDO.Text = submittedAnswer.Text;
        }
    }
}
    

