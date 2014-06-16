using System.Data.Entity.Infrastructure;

namespace Data.Interfaces
{
    /// <summary>
    /// Implementing this interface will allow you to perform pre-save processing.
    /// </summary>
    public interface ISaveHook
    {
        void BeforeSave();
    }
}
