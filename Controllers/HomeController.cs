using System.Web.Mvc;
using KwasantWeb.Filters;

namespace KwasantWeb.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return Redirect("/");
            
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        [RequestParamsEncryptedFilter]
        public ActionResult AboutEnc(int id, string s)
        {
            ViewBag.Message = "Your application description page.";
            ViewBag.Id = id;
            ViewBag.S = s;

            return View("About");
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}