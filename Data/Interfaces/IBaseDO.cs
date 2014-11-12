using System;

namespace Data.Interfaces
{
    public interface IBaseDO
    {
        DateTimeOffset LastUpdated { get; set; }
    }
}