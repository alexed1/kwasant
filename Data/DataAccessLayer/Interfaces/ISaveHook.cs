using System.Data.Entity.Infrastructure;

namespace Data.DataAccessLayer.Interfaces
{
    public interface ISaveHook
    {
        void SaveHook(DbEntityEntry<ISaveHook> entity);
    }
}
