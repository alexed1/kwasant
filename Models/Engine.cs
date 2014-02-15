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

            foreach (var queue in db.Queues)
            {
                
                foreach (var messageId in queue.MessageList)
                {

                    Message curMessage = db.Messages.First(m => m.Id == messageId);
                    if (curMessage.State == MessageState.UNSENT)
                    {
                        try
                        {
                            curMessage.Send(queueRepo);
                            curMessage.State = MessageState.SENT;
                        }
                        catch
                        {
                            throw new ApplicationException("Transmission Failed");
                        }


                    }
                    else throw new ApplicationException("A Sent message was found in the outbound queues. problem. big problem.");
                }

            }

        }
    
    
    
    
    
    
    
    
    
    
    }


   
}