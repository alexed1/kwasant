using System.Data.Entity.Infrastructure;

namespace Data.Interfaces
{
    /// <summary>
    /// Implementing this interface will allow you to perform pre-save and post-save processing.
    /// </summary>
    public interface ISaveHook
    {
        void BeforeSave(IDBContext context);
    }

    public interface IModifyHook
    {
        void OnModify(DbPropertyValues originalValues, DbPropertyValues currentValues, IDBContext context);
    }

    public interface IDeleteHook
    {
        void OnDelete(DbPropertyValues originalValues, IDBContext context);
    }

    public interface ICreateHook
    {
        void BeforeCreate();
        void AfterCreate();
    }
}
