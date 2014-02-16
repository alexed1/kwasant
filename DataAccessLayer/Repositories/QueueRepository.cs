using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Shnexy.Models;
using System.Data.Entity;
using Shnexy.DataAccessLayer.Interfaces;

namespace Shnexy.DataAccessLayer
{
    public class QueueRepository : GenericRepository<Queue>, IQueueRepository
    {
        

        public QueueRepository(IUnitOfWork uow) : base(uow)
        {
            
        }

    }
    
}