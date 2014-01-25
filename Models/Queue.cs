using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using Shnexy.Utilities;

namespace Shnexy.Models
{
    public class Queue
    {
        [Key]
        public int Id { get; set; }
        public Topic Topic;
        
        public List<User> Producers { get; set;}
        public List<User> Consumers;

        private ShnexyDBContext db = new ShnexyDBContext();

        public Queue()
        {
            Producers = new List<User>();
            Consumers = new List<User>();
        }


        public Queue(Topic topic) : this()
        {
            this.Topic = topic;
            
        }
        
        public Queue Get (int queueId) 
        {
             return db.Queues.Find(queueId);
        }

      

        public Queue Load(int topicId)
        {

            return this;
        }

        //adds a user to a queue
        //return error if already in queue
        public void Join(User user, ParticipantType type)
        {
           
            if (!AlreadyParticipating(user))
                switch (type.Name)
                {
                    case "PRODUCER":
                        Producers.Add(user);
                        break;
                    case "CONSUMER":
                        Consumers.Add(user);
                        break;
                    default:
                        throw new ArgumentException("Tried to add an undefined participant type to a queue");
                }
        }

         public void Leave(User CurUser, ParticipantType type)
        {
            switch (type.Name)
            {
                case "PRODUCER":
                    //there's got to be a cleaner way to do this, but if you just call remove on the input User, you get hash code mismatches
                    //possibly should just pass around userID but make the member a User simplifies the rendering.
                    foreach(User user in this.Producers.ToList())
                    {
                        if (user.Id == CurUser.Id)
                            this.Producers.Remove(user);
                    }
                    break;
                case "CONSUMER":
                    foreach (var user in this.Consumers.ToList())
                    {
                        if (user.Id == CurUser.Id)
                            this.Consumers.Remove(user);
                    }
                    break;
                default:
                    throw new ArgumentException("Tried to remove an undefined participant type to a queue");
            }

        }

        private bool AlreadyParticipating(User user)
        {
            if (Producers.Contains(user) || Consumers.Contains(user))
            {
                return true;
            }
            else return false;

        }



    }


}