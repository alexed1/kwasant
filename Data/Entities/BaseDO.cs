using System;

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

    public interface IBaseDO
    {
        DateTimeOffset LastUpdated { get; set; }
        DateTimeOffset CreateDate { get; set; }
    }
}
