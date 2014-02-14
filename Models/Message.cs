using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Shnexy.Utilities;
using System.Diagnostics;
using Shnexy.DataAccessLayer;

namespace Shnexy.Models
{
    public class Message
    {



        public int Id { get; set; }
        public string Body { get; set; }
        public ICollection<Address> RecipientList { get; set; }
        public Address Sender { get; set; }
        public MessageState State;

        private UnitOfWork unitOfWork = new UnitOfWork();

        public Message ()
        {

            RecipientList = new List<Address> { };
            State = MessageState.UNSENT;
        }


        public void Send()
        {

            //validate message

            //build a list of the relevant queues by examining the envelopes of the recipients
            ICollection<String> targetQueues = new List<String>();
            foreach(var address in RecipientList)
            {
                //look at the service name of the address
                if (!targetQueues.Contains(address.ServiceName))
                {
                    targetQueues.Add(address.ServiceName);
                }
            }

            //for each queue, call Queue#Enqueue(messageId)
            foreach(String queueName in targetQueues)
            {
                Queue queue = unitOfWork.QueueRepository.Get(q => q.ServiceName == queueName).First();
                queue.Enqueue(this.Id);               
            }
            unitOfWork.Save();
           
        }
    }
}