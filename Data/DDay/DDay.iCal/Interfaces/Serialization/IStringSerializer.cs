using System.IO;

namespace Data.DDay.DDay.iCal.Interfaces.Serialization
{
    public interface IStringSerializer :
        ISerializer
    {
        string SerializeToString(object obj);
        object Deserialize(TextReader tr);
    }
}
