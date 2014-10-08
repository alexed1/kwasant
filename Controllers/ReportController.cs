using System.Web.Mvc;
using Data.Interfaces;
using KwasantCore.Managers;
using KwasantCore.Services;
using StructureMap;
using Utilities;

namespace KwasantWeb.Controllers
{
    [KwasantAuthorize(Roles = "Admin")]
    public class ReportController : Controller
    {
        //private DataTablesPackager _datatables;
        private Report _report;
        private JsonPackager _jsonPackager;

        public ReportController()
        {
           // _datatables = new DataTablesPackager();
            _report = new Report();
            _jsonPackager = new JsonPackager();
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
                //var jsonResult = Json(_datatables.Pack(_report.Generate(uow, dateRange, type)), JsonRequestBehavior.AllowGet);
                var report = _report.Generate(uow, dateRange, type);
                var jsonResult = Json(_jsonPackager.Pack(report), JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }
        //Display View "History"
        public ActionResult History()
        {
            return View("History");
        }

        //GET: /HistoryReport
        public ActionResult ShowHistoryReport(string primaryCategory, string bookingRequestId, string queryPeriod)
        {
            DateRange dateRange = DateUtility.GenerateDateRange(queryPeriod);
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //var jsonResult = Json(_datatables.Pack(_report.GenerateHistoryReport(uow, dateRange, primaryCategory, bookingRequestId)), JsonRequestBehavior.AllowGet);
                var historyReport = _report.GenerateHistoryReport(uow, dateRange, primaryCategory, bookingRequestId);
                var jsonResult = Json(_jsonPackager.Pack(historyReport), JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }

        //Display partial view "_History" on new window.
        public ActionResult HistoryByBookingRequestId(int bookingRequestID)
        {
            ViewBag.bookingRequestID = bookingRequestID;
            return View("_History");
        }

        //GET: /HistoryReport by BookingRequestId
        public ActionResult ShowHistoryByBookingRequestId(int bookingRequestId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
               // var jsonResult = Json(_datatables.Pack(_report.GenerateHistoryByBookingRequestId(uow, bookingRequestId)), JsonRequestBehavior.AllowGet);
                var historyByBRId = _report.GenerateHistoryByBookingRequestId(uow, bookingRequestId);
                var jsonResult = Json(_jsonPackager.Pack(historyByBRId), JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
             }
        }
       
	}
}