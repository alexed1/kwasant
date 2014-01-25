using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Shnexy.Models;
using Shnexy.Utilities;
using Shnexy.ViewModels;

namespace Shnexy.Controllers
{
    public class QueueController : Controller
    {
        private ShnexyDBContext db = new ShnexyDBContext();

        // GET: /Queue/
        public ActionResult Index()
        {
            ShowQueuesVM curVM = new ShowQueuesVM();
            curVM.Queues = Shnesson.Queues;
            return View(curVM);
        }

        // GET: /Queue/Details/5
        public ActionResult Details(int id)  //removed nullable question mark. not sure about side effects.
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Queue queue = new Queue();
            queue = queue.Get(id);
           
            if (queue == null)
            {
                return HttpNotFound();
            }
            return View(queue);
        }

        // GET: /Queue/Join
        public ActionResult Join(int QueueId, int UserId, string Type)
        {
            Queue targetQueue = Shnesson.Queues.Where(queue => queue.Id == QueueId).First();
            User curUser = new User();
            ParticipantType type = (ParticipantType)Type;
            curUser = curUser.Get(UserId);         
            targetQueue.Join(curUser, type);
            return RedirectToAction("Index");
        }
        // GET: /Queue/Leave
        public ActionResult Leave(int QueueId, int UserId, string Type)
        {
            Queue targetQueue = Shnesson.Queues.Where(queue => queue.Id == QueueId).First();
            User curUser = new User();
            ParticipantType type = (ParticipantType)Type;
            curUser = curUser.Get(UserId);
            targetQueue.Leave(curUser, type);
            return RedirectToAction("Index");
        }


        // GET: /Queue/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /Queue/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="Id")] Queue queue)
        {
            if (ModelState.IsValid)
            {
                db.Queues.Add(queue);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(queue);
        }

        // GET: /Queue/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Queue queue = db.Queues.Find(id);
            if (queue == null)
            {
                return HttpNotFound();
            }
            return View(queue);
        }

        // POST: /Queue/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="Id")] Queue queue)
        {
            if (ModelState.IsValid)
            {
                db.Entry(queue).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(queue);
        }

        // GET: /Queue/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Queue queue = db.Queues.Find(id);
            if (queue == null)
            {
                return HttpNotFound();
            }
            return View(queue);
        }

        // POST: /Queue/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Queue queue = db.Queues.Find(id);
            db.Queues.Remove(queue);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
