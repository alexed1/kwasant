using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using Daemons;
using Data.Entities;
using Data.Interfaces;
using KwasantCore.ExternalServices;
using KwasantCore.Managers;
using KwasantCore.Services;
using KwasantWeb.ViewModels;
using StructureMap;
using Utilities;

namespace KwasantWeb.Controllers
{
    [KwasantAuthorize(Roles = "Admin")]
    public class DiagnosticsController : Controller
    {
        public ActionResult Index()
        {
            var serviceTypes = ServiceManager.GetServices();

            var vm = serviceTypes.Select(st =>
            {
                var info = ServiceManager.GetInformationForService(st);
                var percent = info.Attempts == 0 ? 0 : (int)Math.Round(100.0 * info.Success / info.Attempts);
                var lastUpdated = info.Events.Any() ? GetPrettyDate(info.Events.Max(e => e.Item1)) : "Never";
                var lastSuccess = info.LastSuccess.HasValue ? GetPrettyDate(info.LastSuccess.Value) : "Never";
                var lastFail = info.LastFail.HasValue ? GetPrettyDate(info.LastFail.Value) : "Never";

                bool operational;
                if (!info.LastSuccess.HasValue)
                {
                    operational = !info.LastFail.HasValue;
                }
                else
                {
                    if (info.LastFail.HasValue)
                        operational = info.LastSuccess > info.LastFail;
                    else
                        operational = true;
                }

                return new DiagnosticInfoVM
                {
                    Attempts = info.Attempts,
                    Success = info.Success,
                    Percent = percent,
                    ServiceName = info.ServiceName,
                    LastUpdated = lastUpdated,
                    GroupName = info.GroupName,
                    LastFail = lastFail,
                    LastSuccess = lastSuccess,
                    Operational = operational,
                    Flags = info.Flags,
                    Tests =
                        info.Tests.Select(a => new DiagnosticActionVM { ServerAction = a.Key, DisplayName = a.Value })
                            .ToList(),
                    Actions =
                        info.Actions.Select(a => new DiagnosticActionVM { ServerAction = a.Key, DisplayName = a.Value })
                            .ToList(),
                    Key = info.Key,
                    Events =
                        info.Events.AsEnumerable()
                            .Reverse()
                            .Where(e => !String.IsNullOrEmpty(e.Item2))
                            .Take(15)
                            .Select(e => new DiagnosticEventInfoVM { Date = e.Item1.ToString(), EventName = e.Item2 })
                            .ToList()
                };
            }).ToList();
            return View(vm);
        }

        private static string GetPrettyDate(DateTime d)
        {
            // 1.
            // Get time span elapsed since the date.
            TimeSpan s = DateTime.Now.Subtract(d);

            // 2.
            // Get total number of days elapsed.
            int dayDiff = (int)s.TotalDays;

            // 3.
            // Get total number of seconds elapsed.
            int secDiff = (int)s.TotalSeconds;

            // 5.
            // Handle same-day times.
            if (dayDiff == 0)
            {
                // A.
                // Less than one minute ago.
                if (secDiff < 60)
                {
                    return "Just now";
                }
                // B.
                // Less than 2 minutes ago.
                if (secDiff < 120)
                {
                    return "1 minute ago";
                }
                // C.
                // Less than one hour ago.
                if (secDiff < 3600)
                {
                    return string.Format("{0} minutes ago",
                        Math.Floor((double)secDiff / 60));
                }
                // D.
                // Less than 2 hours ago.
                if (secDiff < 7200)
                {
                    return "1 hour ago";
                }
                // E.
                // Less than one day ago.
                if (secDiff < 86400 * 2)
                {
                    return string.Format("{0} hours ago",
                        Math.Floor((double)secDiff / 3600));
                }
            }
            if (dayDiff < 7)
            {
                return string.Format("{0} days ago",
                    dayDiff);
            }
            if (dayDiff < 31)
            {
                return string.Format("{0} weeks ago",
                    Math.Ceiling((double)dayDiff / 7));
            }
            return string.Format("{0} days ago", dayDiff);
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

        [HttpPost]
        public ActionResult OutboundEmailDaemon_TestGmail(String key)
        {
            return SendTestEmail((uow, curEmail, subjKey, curEmailAddress, subject, message, fromAddress, inboundEmailDaemon, _email) =>
            {
                _email.GenerateBasicMessage(uow, curEmailAddress, subject, message, fromAddress, inboundEmailDaemon.GetUserName());
            });
        }

        [HttpPost]
        public ActionResult OutboundEmailDaemon_TestMandrill(String key)
        {
            return SendTestEmail((uow, curEmail, subjKey, curEmailAddress, subject, message, fromAddress, inboundEmailDaemon, _email) =>
            {
                uow.EnvelopeRepository.ConfigureTemplatedEmail(curEmail, "test_template", null);
            });
        }

        public JsonResult SendTestEmail(Action<IUnitOfWork, EmailDO, string, EmailAddressDO, string, string, string, InboundEmail, Email> configureEmail)
        {
            ServiceManager.LogEvent<OutboundEmail>("Running test...");
            ServiceManager.LogEvent<InboundEmail>("Running test...");

            var inboundEmailDaemon = ServiceManager.GetInformationForService<InboundEmail>().Instance as InboundEmail;
            if (inboundEmailDaemon == null)
                return new JsonResult { Data = false };

            var subjKey = Guid.NewGuid().ToString();

            inboundEmailDaemon.RegisterTestEmailSubject(subjKey);
            bool messageRecieved = false;

            InboundEmail.ExplicitCustomerCreatedHandler testMessageRecieved = subject =>
            {
                if (subjKey == subject)
                    messageRecieved = true;
            };

            InboundEmail.TestMessageRecieved += testMessageRecieved;
            using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Email _email = ObjectFactory.GetInstance<Email>();
                IConfigRepository configRepository = ObjectFactory.GetInstance<IConfigRepository>();
                string fromAddress = configRepository.Get("EmailAddress_GeneralInfo");

                EmailAddressDO curEmailAddress = new EmailAddressDO("ops@kwasant.com");
                EmailDO curEmail = new EmailDO();
                const string message = "This is a test message";
                string subject = subjKey;
                curEmail = _email.GenerateBasicMessage(uow, curEmailAddress, subject, message, fromAddress, inboundEmailDaemon.GetUserName());
                configureEmail(uow, curEmail, subjKey, curEmailAddress, subject, message, fromAddress, inboundEmailDaemon, _email);
                uow.EnvelopeRepository.ConfigureTemplatedEmail(curEmail, "test_template", new Dictionary<string, string> { { "test_key", subjKey } });
                uow.SaveChanges();

                ServiceManager.LogEvent<OutboundEmail>("Queued email to " + inboundEmailDaemon.GetUserName());
            }

            new Thread(() =>
            {
                var startTime = DateTime.Now;
                var maxTime = TimeSpan.FromMinutes(2);
                while (DateTime.Now < startTime.Add(maxTime))
                {
                    if (messageRecieved)
                    {
                        ServiceManager.LogEvent<OutboundEmail>("Test succeeded.");
                        ServiceManager.LogSuccess<OutboundEmail>();

                        ServiceManager.LogEvent<InboundEmail>("Test succeeded.");
                        ServiceManager.LogSuccess<InboundEmail>();
                        return;
                    }

                    Thread.Sleep(100);
                }

                ServiceManager.LogEvent<OutboundEmail>("Test failed.");
                ServiceManager.LogFail<OutboundEmail>();

                ServiceManager.LogEvent<InboundEmail>("Test failed.");
                ServiceManager.LogFail<InboundEmail>();
            }).Start();

            return new JsonResult { Data = true };
        }
    }
}