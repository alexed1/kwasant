using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections;
using Shnexy.Utilities;
using Shnexy.DataAccessLayer;
using Shnexy.DataAccessLayer.Interfaces;

namespace Shnexy.Models
{
    public class Queue : IEntity
    {
        public int Id { get; set; }
        //public string Name { get; set; }
        public virtual PersistableIntCollection MessageList { get; set; }
        public string ServiceName { get; set; }

       // private UnitOfWork unitOfWork = new UnitOfWork();
        private IQueueRepository queueRepo;

        public Queue()
        {
            queueRepo = new QueueRepository(new UnitOfWork(new ShnexyDbContext()));
            MessageList = new PersistableIntCollection { };
        }
        public Queue(IQueueRepository queueRepository)
        {
            queueRepo = queueRepository;
            MessageList = new PersistableIntCollection { };
        }

        //put a message into a queue
        public void Enqueue (int MessageId)
        {
            MessageList.Add(MessageId);
            queueRepo.Update(this);
            queueRepo.Save(this);
            
        }
    }


}