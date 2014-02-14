using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections;
using Shnexy.Utilities;
using Shnexy.DataAccessLayer;

namespace Shnexy.Models
{
    public class Queue
    {
        public int Id { get; set; }
        //public string Name { get; set; }
        public virtual PersistableIntCollection MessageList { get; set; }
        public string ServiceName { get; set; }

        private UnitOfWork unitOfWork = new UnitOfWork();


        public Queue()
        {

            MessageList = new PersistableIntCollection { };
        }

        //put a message into a queue
        public void Enqueue (int MessageId)
        {
            MessageList.Add(MessageId);
            unitOfWork.QueueRepository.Update(this);
            unitOfWork.Save();
            
        }
    }


}