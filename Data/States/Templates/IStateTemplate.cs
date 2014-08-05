using System;

namespace Data.States.Templates
{
    public interface IStateTemplate<T>
    {
        int Id { get; set; }
        String Name { get; set; }
        string ToString();
    }
}
