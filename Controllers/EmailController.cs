using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Data.Constants;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Managers.APIManager.Packagers.Shnexy;
using UtilitiesLib;
using System.Web.Routing;


namespace Shnexy.Controllers
{
    public class EmailController : Controller
    {
        private IUnitOfWork _uow;
        private IBookingRequestRepository curBookingRequestRepository;
        private ShnexyPackager API;

        ShnexyDbContext db = new ShnexyDbContext();

        public EmailController(IUnitOfWork uow)
        {
            _uow = uow;
            curBookingRequestRepository = new BookingRequestRepository(_uow);
            API = new ShnexyPackager();
        }

        // GET: /Email/
        public ActionResult Index()
        {
            return View(curBookingRequestRepository.GetAll().Where(e => e.StatusID == EmailStatusConstants.UNPROCESSED).ToList());    
                                
        }

        // GET: /Email/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BookingRequestDO bookingRequestDO = curBookingRequestRepository.GetByKey(id);

            if (bookingRequestDO == null)
            {
                return HttpNotFound();
            }
            else
            {
                //Redirect to Calendar control to open Booking Agent UI. It takes email id as parameter to which email message will be dispalyed in the left column of Booking Agent UI
                return RedirectToAction("Index", new RouteValueDictionary(new { controller = "Calendar", action = "Index", id = id }));
            }
            //return View(email);
        }

        // GET: /Email/
        [HttpGet]
        public string ProcessGetEmail(string requestString)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            API.UnpackGetEmail(requestString, out param);
            EmailDO thisEmailDO = curBookingRequestRepository.GetByKey(param["Id"].ToInt());
            return API.PackResponseGetEmail(thisEmailDO);
        }
    }
}
