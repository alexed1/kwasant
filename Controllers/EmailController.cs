using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;
using Data.DataAccessLayer.Interfaces;
using Data.DataAccessLayer.Repositories;
using Data.Models;
using DBTools.Managers.APIManager.Packagers.Shnexy;
using UtilitiesLib;

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
            
            API = new ShnexyPackager();
        }



        // GET: /Email/
        [HttpGet]
        public string ProcessGetEmail(string requestString)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            API.UnpackGetEmail(requestString, out param);
            Email thisEmail = curEmailRepo.GetByKey(param["Id"].ToInt());
            return API.PackResponseGetEmail(thisEmail);
        }



        public ActionResult Index()
        {
            
            IEnumerable<Email> curEmails = new List<Email>();
            curEmails = curEmailRepo.GetAll();
            return View("Index", curEmails);
        }

        // GET: /Email/Details/5
        public ActionResult Details(int Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            curEmail = curEmailRepo.GetByKey(Id);
            if (curEmail == null)
            {
                return HttpNotFound();
            }
            return View(curEmail);
        }

       
    }
}
