using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Shnexy.DataAccessLayer.Interfaces;
using Shnexy.Models;

namespace Shnexy.DataAccessLayer.Repositories
{
    public class SerializedEventRepository : GenericRepository<SerializedEvent>, ISerializedEventRepository
    {
        public SerializedEventRepository(IUnitOfWork uow) : base(uow)
        {
            
        }
    }
}


