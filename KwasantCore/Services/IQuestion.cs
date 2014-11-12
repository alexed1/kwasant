using Data.Entities;
using Data.Interfaces;
using KwasantWeb.ViewModels;

namespace KwasantCore.Services
{
    public interface IQuestion
    {
        QuestionDO Update(IUnitOfWork uow, QuestionDO submittedQuestionDO);
    }
}