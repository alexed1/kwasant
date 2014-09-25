using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using KwasantCore.ExternalServices;
using KwasantCore.Managers;
using KwasantWeb.ViewModels;

namespace KwasantWeb.Controllers
{
    [KwasantAuthorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        //
        // GET: /Admin/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Diagnostics()
        {
            var serviceTypes = ServiceManager.GetServices();
            
            var vm = serviceTypes.Select(st =>
            {

                var info = ServiceManager.GetInformationForService(st);
                var percent = info.Attempts == 0 ? 0 : (int) Math.Round(100.0*info.Success/info.Attempts);
                var lastUpdated = info.Events.Any() ? info.Events.Max(e => e.Item1).ToShortTimeString() : "Never";
                return new DiagnosticInfoVM
                {
                    Attempts = info.Attempts,
                    Success = info.Success,
                    Percent = percent,
                    ServiceName = info.ServiceName,
                    LastUpdated = lastUpdated,
                    Events = info.Events.OrderByDescending(ev => ev.Item1).Select(e => new DiagnosticEventInfoVM { Date = e.Item1.ToShortTimeString(), EventName = e.Item2}).ToList()
                };
            }).ToList();
            return View(vm);
        }

        public ActionResult Dashboard()
        {
            return View("Index");
        }
    }
}