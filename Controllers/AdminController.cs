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
using KwasantICS.DDay.iCal.Utility;
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


        //Generate incident report
        [HttpGet]
        public ActionResult ShowIncidentReport(string queryPeriod)
        {
            List<IncidentDO> incidentDOs = new List<IncidentDO>();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                if (queryPeriod=="all")
                {
                    incidentDOs = uow.IncidentRepository.GetAll().ToList();
                }
                else
                {
                  DateUtil dateUtil = new DateUtil();
                  DateTimeOffset dateTimeOffset = dateUtil.GenerateDateRange(queryPeriod);
                  incidentDOs = uow.IncidentRepository.GetAll().Where(x => x.CreateTime.DateTime >= dateTimeOffset.DateTime).ToList();
                }
                var jsonResult = Json(_datatables.Pack(incidentDOs), JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }

    }
}