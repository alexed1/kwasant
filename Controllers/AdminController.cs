using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Daemons;
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
                    GroupName = info.GroupName,
                    Flags = info.Flags,
                    Actions = new List<DiagnosticActionVM> { 
                        new DiagnosticActionVM { DisplayName = "Start", ServerAction = "StartDaemon" }, 
                        new DiagnosticActionVM { DisplayName = "Stop", ServerAction = "StopDaemon" }
                    },
                    Key = info.Key,
                    Events = info.Events.AsEnumerable().Reverse().Take(5).Select(e => new DiagnosticEventInfoVM { Date = e.Item1.ToString(), EventName = e.Item2}).ToList()
                };
            }).ToList();
            return View(vm);
        }

        [HttpPost]
        public ActionResult StartDaemon(String key)
        {
            var daemon = ServiceManager.GetInformationForService(key).Instance as Daemon;
            if (daemon != null)
            {
                daemon.Start();
                return new JsonResult { Data = true };
            }
            return new JsonResult { Data = false };
        }

        [HttpPost]
        public ActionResult StopDaemon(String key)
        {
            var daemon = ServiceManager.GetInformationForService(key).Instance as Daemon;
            if (daemon != null)
            {
                daemon.Stop();
                return new JsonResult { Data = true };
            }
            return new JsonResult { Data = false };
        }

        public ActionResult Dashboard()
        {
            return View("Index");
        }
    }
}