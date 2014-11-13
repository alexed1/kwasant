using System;
using Data.Interfaces;

namespace Data.Entities
{
    public class BaseDO : IBaseDO
    {
        public DateTimeOffset LastUpdated { get; set; }
    }
}
