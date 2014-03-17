using S22.Imap;
using Shnexy.DataAccessLayer;
using Shnexy.DataAccessLayer.Repositories;
using Shnexy.Fixtures;
using Shnexy.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;


namespace Shnexy.Controllers
{
    public class EngineController : Controller
    {
        public IEmailRepository _emailRepo;
        //public IMessageRepository messageRepo;

        public EngineController(IEmailRepository emailRepo)
        {
            _emailRepo = emailRepo;
        }

        //start the system
        public ActionResult Bootstrap()
        {

            //queueRepo = new QueueRepository(new UnitOfWork(new ShnexyDbContext()));
            //messageRepo = new MessageRepository(new UnitOfWork(new ShnexyDbContext()));
            ////update database
            ////Seed.AddMessage(messageRepo);




            //Address address806 = new Address();
            //address806.Body = "14158067915";
            //address806.ServiceName = "WhatsApp";

            //Address address871 = new Address();
            //address871.Body = "14158710872";
            //address871.ServiceName = "WhatsApp";

            //Message message = messageRepo.GetAll().First();
            //message.RecipientList = new List<Address>();
            //message.RecipientList.Add(address806);
            //message.Sender = address871;
            //message.Body = "45 hoo hawHello, Come here, please! ";
            //message.Id = 50;


            //message.TestSend();
            //message.Send(queueRepo);

            string username = "alexlucre1";
            string password = "lucrelucre";
            IEnumerable<MailMessage> messages;
            // Connect on port 993 using SSL.
            using (ImapClient Client = new ImapClient("imap.gmail.com", 993, username, password, AuthMethod.Login, true))
            {
                Debug.WriteLine("We are connected!");
                IEnumerable<uint> uids = Client.Search(SearchCondition.Unseen());
                messages = Client.GetMessages(uids);
            }
            foreach (var message in messages)
            {
                var curEmail = new Email(message, _emailRepo);
                curEmail.Save();
            }
            return RedirectToAction("Index", "Admin");

        }

        public ActionResult ProcessQueues()
        {

            Engine curEngine = new Engine();
            curEngine.ProcessQueues();
            return View();
        }
	}
}