using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Shnexy.Utilities;
using System.Diagnostics;

namespace Shnexy.Models
{
    public class Message
    {



        public int Id { get; set; }
        public string Body { get; set; }
        public ICollection<Address> RecipientList { get; set; }
        public Address Sender { get; set; }
        public MessageState State;

        public Message ()
        {

            RecipientList = new List<Address> { };
            State = MessageState.UNSENT;
        }


        public void Send()
        {

            //validate message
            //build a list of the relevant queues by examining the envelopes of the recipients
            //for each queue, call Queue#Enqueue(messageId)
           
        }
    }
}