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
    

