using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using Shnexy.Utilities;

namespace Shnexy.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        //public UserCallState CallState;
        private ShnexyDBContext db = new ShnexyDBContext();



        public User Get (int userId)
        {
            return db.Users.Find(userId);

        }

        //return a list of all of the queues for which this user is currently waiting to participate as a Producer on a Call
        public List<Queue> GetProducerQueues()
        {
            List<Queue> ProducerQueues = new List<Queue> {};
            //this should probably be done with a query but the syntax defeated me
            foreach (var queue in  Shnesson.Queues)
            {
                foreach (var user in queue.Producers)
                {
                    if (user.Id == this.Id)
                        ProducerQueues.Add(queue);

                }
                
            }
            return ProducerQueues;


        }
        //return a list of all of the queues for which this user is currently waiting to participate as a Producer on a Call
        public List<Queue> GetConsumerQueues()
        {
            List<Queue> ConsumerQueues = new List<Queue> { };
            //this should probably be done with a query but the syntax defeated me
            foreach (var queue in Shnesson.Queues)
            {
                foreach (var user in queue.Consumers)
                {
                    if (user.Id == this.Id)
                        ConsumerQueues.Add(queue);

                }

            }
            return ConsumerQueues;
        }

        //return a list of all of the queues for which this user is currently waiting to participate as a Producer on a Call
        //There are probably much better ways to do this, and this may have serious performance implications for larger sets.
        public List<Queue> GetAvailableQueues(List<Queue> ConQueue, List<Queue> ProQueue )
        {
            List<Queue> AvailableQueues = new List<Queue> { };
            AvailableQueues = Shnesson.Queues.Except(ConQueue).ToList();
            AvailableQueues = Shnesson.Queues.Except(ProQueue).ToList();
            return AvailableQueues;
        }

    }

    public class ShnexyDBContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Queue> Queues { get; set; }
        public DbSet<Topic> Topics { get; set; }
    }
}