using Shnexy.DataAccessLayer;
using Shnexy.Fixtures;
using Shnexy.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace Shnexy.Controllers
{
    public class EngineController : Controller
    {
        public IQueueRepository queueRepo;
        public IMessageRepository messageRepo;
        //start the system
        public ActionResult Bootstrap()
        {

            queueRepo = new QueueRepository(new UnitOfWork(new ShnexyDbContext()));
            messageRepo = new MessageRepository(new UnitOfWork(new ShnexyDbContext()));
            //update database
            //Seed.AddMessage(messageRepo);




            Message message = messageRepo.GetAll().First();
            //message.RecipientList.Add(address806);
            //message.Send(queueRepo);

            Debug.WriteLine("debug output from AdminController");
            ProcessStartInfo info = new ProcessStartInfo("D:/dev/shnexy/ShnexyMTA/bin/Debug/ShnexyMTA.exe");

            using (Process process = Process.Start(info))
            {

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