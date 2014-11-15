using System;
using Data.Interfaces;

namespace Data.Entities
{
    public class BaseDO : IBaseDO, ICreateHook
    {
        public DateTimeOffset LastUpdated { get; set; }
        public DateTimeOffset CreateDate { get; set; }

        public virtual void BeforeCreate()
        {
            CreateDate = DateTimeOffset.Now;
        }

        public virtual void AfterCreate()
        {
        }
    }
}
