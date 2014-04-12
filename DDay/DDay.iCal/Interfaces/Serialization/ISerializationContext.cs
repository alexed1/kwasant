using IServiceProvider = Data.DDay.DDay.iCal.Interfaces.General.IServiceProvider;

namespace Data.DDay.DDay.iCal.Interfaces.Serialization
{
    public interface ISerializationContext : 
        IServiceProvider
    {
        void Push(object item);
        object Pop();
        object Peek();        
    }
}
