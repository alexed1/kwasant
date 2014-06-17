using System.Collections.Generic;
using System.Web.Mvc;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Managers.APIManager.Packagers.Kwasant;
using KwasantCore.Managers.IdentityManager;
using StructureMap;
using Utilities;

namespace KwasantWeb.Controllers
{
    [KwasantAuthorize(Roles = "Admin")]
    public class EmailController : Controller
    {
        private IUnitOfWork _uow;
        private IBookingRequestRepository curBookingRequestRepository;
        private KwasantPackager API;


        public EmailController()
        {
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();
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
