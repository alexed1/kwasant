using System.Text;

namespace Data.DDay.DDay.iCal.Interfaces.Serialization
{
    public interface IEncodingStack        
    {
        Encoding Current { get; }
        void Push(Encoding encoding);
        Encoding Pop();
    }
}
