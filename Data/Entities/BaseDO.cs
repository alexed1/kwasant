using System;
using Data.Interfaces;

namespace Data.Entities
{
    public class BaseDO : IBaseDO
    {
        public BaseDO()
        {
            // will be moved to UnitOfWork.SaveChanges together with LastUpdated assignment
            CreateDate = DateTimeOffset.Now;
        }

        public DateTimeOffset LastUpdated { get; set; }
        public DateTimeOffset CreateDate { get; set; }
    }
        DateTimeOffset CreateDate { get; set; }
    }
