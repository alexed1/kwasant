using System.Web.Mvc;
using KwasantCore.Managers.IdentityManager;

namespace KwasantWeb.Controllers
{
    public class AdminController : Controller
    {
        //
        // GET: /Admin/
        [KwasantAuthorizeAttribute(Roles = "Admin")]
        public ActionResult Index()
        {
            //var engine = new Engine();
            //engine.ProcessQueues(); database needs the messagelist initialized from null for this to work

            return View();
        }

        //public ActionResult ProcessBookings()
        //{
        //    //get all Bookings with status = "unprocessed"
        //    //foreach Booking, process it
        //}
    }
}