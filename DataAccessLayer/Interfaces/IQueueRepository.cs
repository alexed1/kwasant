using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Shnexy.Models;
using Shnexy.DataAccessLayer.Interfaces;

namespace Shnexy.DataAccessLayer
{
    public interface IQueueRepository : IRepository<Queue>, IDisposable
    {
        
    }
}