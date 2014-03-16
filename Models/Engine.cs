using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Shnexy.Utilities;
using Shnexy.DataAccessLayer;

namespace Shnexy.Models
{
    public class Engine
    {
        public int Id;

        private ShnexyDbContext db = new ShnexyDbContext();
        private QueueRepository queueRepo = new QueueRepository(new UnitOfWork(new ShnexyDbContext()));

        public void ProcessQueues()
        {
            //get all queues
            //for each queue
            //    get all messages
            //    if they're unsent, send them, remove them from the queue, and change their status to sent

           

        }
    
    
    
    
    
    
    
    
    
    
    }


   
}