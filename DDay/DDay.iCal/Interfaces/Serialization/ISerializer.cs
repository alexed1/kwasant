using System;
using System.Text;
using System.IO;
using IServiceProvider = Data.DDay.DDay.iCal.Interfaces.General.IServiceProvider;

namespace Data.DDay.DDay.iCal.Interfaces.Serialization
{    
    public interface ISerializer :
        IServiceProvider
    {
        ISerializationContext SerializationContext { get; set; }        

        Type TargetType { get; }        
        void Serialize(object obj, Stream stream, Encoding encoding);
        object Deserialize(Stream stream, Encoding encoding);
    }
}
