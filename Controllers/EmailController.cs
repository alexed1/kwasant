using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.Routing;
using Data.Entities;
using Data.Entities.Enumerations;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Managers.APIManager.Packagers.Kwasant;
using Utilities;
using Utilities.Logging;

namespace KwasantWeb.Controllers
{
    public class EmailController : Controller
    {
        private IUnitOfWork _uow;
        private IBookingRequestRepository curBookingRequestRepository;
        private KwasantPackager API;

        KwasantDbContext db = new KwasantDbContext();

        public EmailController(IUnitOfWork uow)
        {
            _uow = uow;
            curBookingRequestRepository = _uow.BookingRequestRepository;
            API = new KwasantPackager();
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
