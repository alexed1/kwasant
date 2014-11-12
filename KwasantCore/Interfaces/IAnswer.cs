using Data.Entities;
using Data.Interfaces;
using KwasantWeb.ViewModels;

namespace KwasantCore.Interfaces
{
    public interface IAnswer
    {
        AnswerDO Update(IUnitOfWork uow, AnswerDO submittedAnswerDO);
    }
}