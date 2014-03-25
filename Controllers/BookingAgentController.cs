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
    public class BookingAgentController : Controller
    {
        private ShnexyDbContext db = new ShnexyDbContext();

        // GET: /Email/
        public ActionResult Index()
        {
            //return View(db.Emails.ToList());

            return View();
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
