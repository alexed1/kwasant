using System;

namespace Data.Entities.Constants
{
    public interface IConstantRow<T>
    {
        int Id { get; set; }
        String Name { get; set; }
        string ToString();
    }
}
