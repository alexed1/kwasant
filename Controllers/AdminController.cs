using System.Web.Mvc;
using KwasantCore.Managers.IdentityManager;
using Data.Interfaces;
using Data.Entities;
using StructureMap;
using Data.Repositories;
using System.Collections.Generic;
using System.Linq;
using System;
using KwasantCore.Managers.APIManager.Packagers.DataTable;
using System.Data.Entity;
namespace KwasantWeb.Controllers
{
   // [KwasantAuthorizeAttribute(Roles = "Admin")]
    public class AdminController : Controller
    {
         private DataTablesPackager _datatables;

         public AdminController()
        {
            _datatables = new DataTablesPackager();
        }
        //
        // GET: /Admin/
        public ActionResult Index()
        {
            //var engine = new Engine();
            //engine.ProcessQueues(); database needs the messagelist initialized from null for this to work

            return View();
        }

        public ActionResult Incident()
        {
            return View();
        }



        [HttpGet]
        public ActionResult ShowIncidentReport(bool all, int lastMinutes, bool lastHour, bool lastDay, bool lastweek)
        {
            var sssss = DateTime.Now.AddHours(-1).ToUniversalTime();
            List<IncidentDO> incidentDOs = new List<IncidentDO>();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                if (all == true)
                {
                    incidentDOs = uow.IncidentRepository.GetAll().ToList();
                }
                else if (lastMinutes !=-1)
                {
                    var sss=DateTime.Now.AddMinutes(lastMinutes).ToUniversalTime();
                 
                    incidentDOs = uow.IncidentRepository.GetAll().Where(x => x.CreateTime.DateTime >= DateTime.Now.AddMinutes(lastMinutes).ToUniversalTime()).ToList();
                }
                else if (lastHour == true)
                {
                    incidentDOs = uow.IncidentRepository.GetAll().Where(x => x.CreateTime.DateTime >= DateTime.Now.AddHours(-1).ToUniversalTime()).ToList();
                }
                else if (lastDay == true)
                {
                    incidentDOs = uow.IncidentRepository.GetAll().Where(x => x.CreateTime.Date == DateTimeOffset.UtcNow.Date.AddDays(-1)).ToList();
                  
                }
                else if (lastweek == true)
                {
                    incidentDOs = uow.IncidentRepository.GetAll().Where(x => x.CreateTime > DateTimeOffset.UtcNow.AddDays(-7)).ToList();
                }
                var jsonResult = Json(_datatables.Pack(incidentDOs), JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }
       
        //public ActionResult ProcessBookings()
        //{
        //    //get all Bookings with status = "unprocessed"
        //    //foreach Booking, process it
        //}
    }
}