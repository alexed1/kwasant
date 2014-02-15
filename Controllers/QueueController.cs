using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Shnexy.Models;
using Shnexy.DataAccessLayer;

namespace Shnexy.Controllers
{
    public class QueueController : Controller
    {
        private IQueueRepository queueRepository;

        public QueueController()
        {
           // this.queueRepository = new QueueRepository(new ShnexyDbContext());

        }

        public QueueController(IQueueRepository queueRepository)
        {
            this.queueRepository = queueRepository;
        }

        // GET: /Queue/
        public ActionResult Index()
        {
            var queues = queueRepository.GetQuery();
            return View(queues);
        }

        // GET: /Queue/Details/5
        public ViewResult Details(int id)
        {
           
            Queue queue = queueRepository.GetByKey(id); // db.Queues.Find(id);
           
            return View(queue);
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
        public ActionResult Create([Bind(Include="Id,ServiceName")] Queue queue)
        {
            if (ModelState.IsValid)
            {
                queueRepository.Add(queue); //db.Queues.Add(queue);
                queueRepository.Save(queue); //db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(queue);
        }

        // GET: /Queue/Edit/5
        public ActionResult Edit(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Queue queue = queueRepository.GetByKey(id); // db.Queues.Find(id);
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
        public ActionResult Edit([Bind(Include="Id,ServiceName")] Queue queue)
        {
            if (ModelState.IsValid)
            {
                queueRepository.Update(queue); // db.Entry(queue).State = EntityState.Modified;
                queueRepository.Save(queue); // db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(queue);
        }

        // GET: /Queue/Delete/5
        public ActionResult Delete(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Queue queue = queueRepository.GetByKey(id);  //db.Queues.Find(id);
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
            Queue queue = queueRepository.GetByKey(id);  //db.Queues.Find(id);
            queueRepository.Remove(queue); // db.Queues.Remove(queue);
            //queueRepository.Save(queue); // db.SaveChanges();

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                queueRepository.Dispose(); // db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
