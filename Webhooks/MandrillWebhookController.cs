using System;
using System.Web.Mvc;
using KwasantCore.Services;

namespace KwasantWeb.Webhooks
{
    public class MandrillWebhookController : Controller
    {
        //
        // GET: /MandrillWebhook/
        public ActionResult Index()
        {
            var request = Request.Form["mandrill_events"];
            if (!String.IsNullOrEmpty(request))
            {
                Email.HandleWebhookResponse(request);
            }
            return View();
        }
    }
}