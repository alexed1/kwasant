using System;
using System.Collections.Generic;
using System.Text;

namespace Shnexy.DDay.iCal
{
    public interface ISerializationProcessor<T>
    {
        void PreSerialization(T obj);
        void PostSerialization(T obj);
        void PreDeserialization(T obj);
        void PostDeserialization(T obj);
    }
}
