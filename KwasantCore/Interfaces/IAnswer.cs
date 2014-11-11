using Data.Entities;
using Data.Interfaces;
using KwasantWeb.ViewModels;

namespace KwasantCore.Interfaces
{
    public interface IAnswer
    {
        AnswerDO GetOrCreate(IUnitOfWork uow, int? curAnswerID);
        void Update(IUnitOfWork uow, NegotiationAnswerVM submittedAnswer);
    }
}