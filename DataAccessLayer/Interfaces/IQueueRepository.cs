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
        //IEnumerable<Queue> GetQueues();
        //Queue GetQueueById(int queueId);
        //void InsertQueue(Queue queue);
        //void DeleteQueue(int queueID);
       // void UpdateQueue(Queue queue);
       // void Save();
    }
}