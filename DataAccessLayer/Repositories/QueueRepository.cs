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
        
        //private ShnexyDbContext context;

        public QueueRepository(IUnitOfWork uow) : base(uow)
        {
            
        }

        //public IEnumerable<Queue> GetQueues()
        //{

        //    return dbSet.ToList();
        //}

        //public Queue GetQueueById(int id)
        //{
        //    return context.Queues.Find(id);
        //}

        //public void InsertQueue(Queue queue)
        //{
        //    context.Queues.Add(queue);
        //}

        //public void DeleteQueue(int queueID)
        //{
        //    Queue queue = context.Queues.Find(queueID);
        //    context.Queues.Remove(queue);
        //}

        //public void UpdateQueue(Queue queue)
        //{
        //    context.Entry(queue).State = EntityState.Modified;
        //}

        //public void Save()
        //{
        //    context.SaveChanges();
        //}

        //private bool disposed = false;

        //protected virtual void Dispose(bool disposing)
        //{
        //    if (!this.disposed)
        //    {
        //        if (disposing)
        //        {
        //            context.Dispose();
        //        }
        //    }
        //    this.disposed = true;
        //}

        //public void Dispose()
        //{
        //    Dispose(true);
        //    GC.SuppressFinalize(this);
        //}
    }
    
}