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

namespace Shnexy.Controllers
{
    public class EmailController : Controller
    {
        

        private IUnitOfWork _uow;

        private Email curEmail;

        public EmailController(IUnitOfWork uow)
        {
            _uow = uow;
            curEmail = new Email(new EmailRepository(_uow)); //why is repo null?
        }



        // GET: /Email/
        public ActionResult Index()
        {
            
            List<Email> curEmails = new List<Email>();
            curEmails = curEmail.GetAll().ToList();
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
