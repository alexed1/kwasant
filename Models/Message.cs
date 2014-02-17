using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Shnexy.Utilities;
using System.Diagnostics;
using Shnexy.DataAccessLayer;
using Shnexy.DataAccessLayer.Interfaces;
using WhatsAppApi;
using WhatsTest;
using WhatsAppPort;

namespace Shnexy.Models
{
    public class Message : IEntity
    {



        public int Id { get; set; }
        public string Body { get; set; }
        public ICollection<Address> RecipientList { get; set; }
        public Address Sender { get; set; }
        public MessageState State;

        private IMessageRepository messageRepo;
        private IQueueRepository queueRepo;

        public Message()
        {
            messageRepo = new MessageRepository(new UnitOfWork(new ShnexyDbContext()));
            
        }
        public Message (IMessageRepository messageRepository)
        {

            messageRepo = messageRepository;

            RecipientList = new List<Address>();
            State = MessageState.UNSENT;
        }

        public void TestSend()
        {

            string nickname = "WhatsApiNet";
            string sender = "14158710872"; // Mobile number with country code (but without + or 00)
            string password = "vvAfia/L8DxRsw6l7gRS8L0Tg5U=";//v2 password
            string target = "14158067915";// Mobile number to send the message to

            //WhatSocket.Instance.Message(this.user.WhatsUser.GetFullJid(), txtBxSentText.Text);
            WhatsTest.Program curProg = new WhatsTest.Program();
            WhatsTest.Program.Process(sender, password, nickname, target, this.Body);
            Debug.WriteLine("message processed");
        }
        public void Send(IQueueRepository queueRepo)
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
                Queue queue = queueRepo.GetQuery().Where(q => q.ServiceName == queueName).First();
                queue.Enqueue(this.Id);               
            }
            messageRepo.UnitOfWork.SaveChanges();

           
        }
    }
}