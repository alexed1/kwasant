using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Data.DataAccessLayer.Interfaces;
using Data.DataAccessLayer.Repositories;
using Data.Models;
using Data.Services.APIManager.Packagers.Shnexy;
using UtilitiesLib;
using Data.DataAccessLayer;
using Data.DataAccessLayer.Infrastructure;
using System.Web.Routing;
using Shnexy.Fixtures;


namespace Shnexy.Controllers
{
    public class EmailController : Controller
    {
        private IUnitOfWork _uow;

        private IEmail curEmail;
        private IEmailRepository curEmailRepo;
        private ShnexyPackager API;

        ShnexyDbContext db = new ShnexyDbContext();

        // GET: /Email/
        public ActionResult Index()
        {            
            return View(curEmailRepo.GetAll().Where(e => e.Status == "Unprocess").ToList());            
        }

        // GET: /Email/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Email email = curEmailRepo.GetByKey(id);

            if (email == null)
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

        public EmailController(IUnitOfWork uow)
        {
            _uow = uow;
            curEmailRepo = new EmailRepository(_uow);
            curEmail = new Email(curEmailRepo); //why is repo null?
            API = new ShnexyPackager();
        }



        // GET: /Email/
        [HttpGet]
        public string ProcessGetEmail(string requestString)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            API.UnpackGetEmail(requestString, out param);
            Email thisEmail = curEmail.GetByKey(param["Id"].ToInt());
            return API.PackResponseGetEmail(thisEmail);
        }
    }
}
