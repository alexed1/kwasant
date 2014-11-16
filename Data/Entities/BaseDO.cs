using System;
using System.Data.Entity.Infrastructure;
using Data.Infrastructure;
using Data.Interfaces;

namespace Data.Entities
{
    public class BaseDO : IBaseDO, IModifyHook
    {
        public DateTimeOffset LastUpdated { get; set; }

        public virtual void OnModify(DbPropertyValues originalValues, DbPropertyValues currentValues)
        {
            this.DetectStateUpdates(originalValues, currentValues);
        }
    }
}
