using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Data.Interfaces;
using KwasantCore.Managers;
using KwasantCore.Managers.APIManager.Packagers.DataTable;
using KwasantCore.Services;
using StructureMap;
using Utilities;

namespace KwasantWeb.Controllers
{
    [KwasantAuthorize(Roles = "Admin")]
    public class ReportController : Controller
    {
        private DataTablesPackager _datatables;
        private Report _report;

        public ReportController()
        {
            _datatables = new DataTablesPackager();
            _report = new Report();
        }

        //
        // GET: /Report/
        public ActionResult Index(string type)
        {
            ViewBag.type = type;
            switch (type)
            {
                case "usage" :
                    ViewBag.Title = "Usage Report";
                    break;
                case "incident":
                    ViewBag.Title = "Incident Report";
                    break;
            }
            return View();
        }

        public ActionResult ShowReport(string queryPeriod, string type)
        {
            DateRange dateRange = DateUtility.GenerateDateRange(queryPeriod);
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var jsonResult = Json(_datatables.Pack(_report.Generate(uow, dateRange, type)), JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }
	}
}