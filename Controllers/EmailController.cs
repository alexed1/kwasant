using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Shnexy.DataAccessLayer.Interfaces;
using Shnexy.DataAccessLayer.Repositories;
using Shnexy.Models;
using Shnexy.DataAccessLayer;
using Shnexy.Services.APIManagement.Packagers.Shnexy;
using Shnexy.Utilities;
using StructureMap;

namespace Shnexy.Controllers
{
    public class EmailController : Controller
    {
        

        private IUnitOfWork _uow;

        private IEmail curEmail;
        private IEmailRepository curEmailRepo;
        private ShnexyPackager API;
        

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



        public ActionResult ShowAll()
        {
            
            IEnumerable<Email> curEmails = new List<Email>();
            curEmails = curEmail.GetAll();
            return View(curEmails);
        }

        // GET: /Email/Details/5
        public ActionResult Details(int Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            curEmail = curEmail.GetByKey(Id);
            if (curEmail == null)
            {
                return HttpNotFound();
            }
            return View(curEmail);
        }

       
    }
}
