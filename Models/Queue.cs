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

        public string MessageList { get; set; }
        public string ServiceName { get; set; }
        public string foo { get; set; }


        private IQueueRepository queueRepo;

        public Queue()
        {
            queueRepo = new QueueRepository(new UnitOfWork(new ShnexyDbContext()));
            MessageList = "";
        }
        public Queue(IQueueRepository queueRepository)
        {
            queueRepo = queueRepository;
            MessageList = "";
        }

        //put a message into a queue
        public void Enqueue (int MessageId)
        {
            MessageList = MessageList + "," + MessageId.ToStr();  //this adds a leading comma to empty strings. may need to deal here. probably should.
            queueRepo.Update(this);
            queueRepo.UnitOfWork.SaveChanges();
                

            
        }
    }




}