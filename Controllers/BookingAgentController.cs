using System;
using System.Web.Mvc;
using DayPilot.Web.Mvc.Json;
using Shnexy.Controllers.Data;


namespace Shnexy.Controllers
{
    public class BookingAgentController : Controller
    {
        //private ShnexyDbContext db = new ShnexyDbContext(); Use injection

        // GET: /Email/
        public ActionResult Index()
        {
            //return View(db.Emails.ToList());

            return View();
        }

        //[AcceptVerbs(HttpVerbs.Post)]
        //public ActionResult New(FormCollection form)
        //{
        //    DateTime start = Convert.ToDateTime(form["Start"]);
        //    DateTime end = Convert.ToDateTime(form["End"]);
        //    new EventManager(this).EventCreate(start, end, form["Text"], null);
        //    return JavaScript(SimpleJsonSerializer.Serialize("OK"));
        //}        

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
