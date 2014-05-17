using System.Web.Mvc;

namespace KwasantWeb.Controllers
{
    public class KwasantAuthorizeAttribute : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext context)
        {
            // redirect to Error page
            context.Result = new RedirectResult("/UnauthorizedRequest.html");
        }
    }

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