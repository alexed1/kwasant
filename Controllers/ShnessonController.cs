using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Shnexy.Models;

namespace Shnexy.Controllers
{
    public class ShnessonController : Controller
    {
        private ShnexyDBContext db = new ShnexyDBContext();
        //
        // GET: /Session/
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Bootstrap()
        {

        
            //Create a queue for each defined topic and add it to the master list of queues
            List<Topic> topics = db.Topics.ToList();
            int counter = 1;
            foreach (Topic topic in topics)
            {
                Queue curQueue = new Queue(topic);
                curQueue.Id = counter;
                Shnesson.Queues.Add(curQueue);
                counter += 1;
            }
            return RedirectToAction("Index", "Admin");
           
        }

	}
}