using System;

namespace Data.Entities
{
    public class BaseDO : IBaseDO
    {
        public DateTimeOffset LastUpdated { get; set; }
    }

    public interface IBaseDO
    {
        DateTimeOffset LastUpdated { get; set; }
    }
}
