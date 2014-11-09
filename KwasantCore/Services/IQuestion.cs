using Data.Entities;
using Data.Interfaces;
using KwasantWeb.ViewModels;

namespace KwasantCore.Services
{
    public interface IQuestion
    {
        QuestionDO GetOrCreate(IUnitOfWork uow, int? curQuestionID);
        void Update(IUnitOfWork uow, NegotiationQuestionVM submittedQuestion, NegotiationDO curNegotiationDO);
        void RemoveDeletedAnswers(IUnitOfWork uow, QuestionDO curQuestionDO, NegotiationQuestionVM submittedQuestion);
    }
}