namespace Data.Interfaces
{
    /// <summary>
    /// Implementing this interface will allow you to perform pre-save and post-save processing.
    /// </summary>
    public interface ISaveHook
    {
        void BeforeSave();

    }

    public interface ICreateHook
    {
        void AfterCreate();
    }
}
