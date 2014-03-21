using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Shnexy.DataAccessLayer.Interfaces;
using Shnexy.Models;
using Shnexy.DataAccessLayer;

namespace Shnexy.Controllers
{
    public class EmailController : Controller
    {
        

        private IUnitOfWork _uow;
        private ShnexyDbContext db;


        public EmailController(IUnitOfWork uow)
        {
            _uow = uow;
            db = (ShnexyDbContext)uow.Db; //don't know why we have to have an explicit cast here
        }



        // GET: /Email/
        public ActionResult Index()
        {
            return View(db.Emails.ToList());
        }

        // GET: /Email/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Email curEmail = db.Emails.Find(id);
            if (curEmail == null)
            {
                return HttpNotFound();
            }
            return View(curEmail);
        }

        // GET: /Email/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /Email/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="Id,Body,Subject")] Email curEmail)
        {
            if (ModelState.IsValid)
            {
                db.Emails.Add(curEmail);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(curEmail);
        }

        // GET: /Email/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Email curEmail = db.Emails.Find(id);
            if (curEmail == null)
            {
                return HttpNotFound();
            }
            return View(curEmail);
        }

        // POST: /Email/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="Id,Body,Subject")] Email curEmail)
        {
            if (ModelState.IsValid)
            {
                db.Entry(curEmail).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(curEmail);
        }

        // GET: /Email/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Email curEmail = db.Emails.Find(id);
            if (curEmail == null)
            {
                return HttpNotFound();
            }
            return View(curEmail);
        }

        // POST: /Email/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Email curEmail = db.Emails.Find(id);
            db.Emails.Remove(curEmail);
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
