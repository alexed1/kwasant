using System.Data.Entity.Infrastructure;

namespace Data.Interfaces
{
    /// <summary>
    /// Implementing this interface will allow you to perform pre-save and post-save processing.
    /// </summary>
    public interface ISaveHook
    {
        void BeforeSave(IUnitOfWork uow);
    }

    public interface IModifyHook
    {
        void OnModify(DbPropertyValues originalValues, DbPropertyValues currentValues, IUnitOfWork uow);
    }

    public interface IDeleteHook
    {
        void OnDelete(DbPropertyValues originalValues, IUnitOfWork uow);
    }

    public interface ICreateHook
    {
        void BeforeCreate();
        void AfterCreate();
    }
}
