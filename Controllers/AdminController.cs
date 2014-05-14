using System.Web.Mvc;

namespace KwasantWeb.Controllers
{
    public class AdminController : Controller
    {
        //
        // GET: /Admin/
        public ActionResult Index()
        {
            //var engine = new Engine();
            //engine.ProcessQueues(); database needs the messagelist initialized from null for this to work
            if (User.IsInRole("Admin") == false)
            {
                ViewBag.Alert = "Admin credentials are required to access this page.";
            }

            return View();
        }

        //public ActionResult ProcessBookings()
        //{
        //    //get all Bookings with status = "unprocessed"
        //    //foreach Booking, process it
        //}
	}
}