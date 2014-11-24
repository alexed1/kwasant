using System;
using System.Net;
using System.Web.Mvc;
using KwasantCore.Managers;
using KwasantWeb.AlertQueues;

namespace KwasantWeb.Controllers
{
    [KwasantAuthorize]
    public class AlertingController : Controller
    {
        [HttpPost]
        public ActionResult RegisterInterestInPageUpdates(string eventName, int objectID)
        {
            if (String.IsNullOrEmpty(eventName))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            
            var guid = Guid.NewGuid().ToString();

            if (Session[guid] == null)
            {
                var queue = PersonalAlertQueues.GetQueueByName(eventName);
                queue.ObjectID = objectID;

                Session[guid] = queue;
            } 
            
            return Json(guid);
        }

        [HttpPost]
        public ActionResult RequestUpdate(string guid)
        {
            if (String.IsNullOrEmpty(guid))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var queue = Session[guid] as IPersonalAlertQueue;

            if (queue == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            return Json(queue.GetUpdates());
        }

        [HttpPost]
        public ActionResult RegisterInterestInUserUpdates(string eventName)
        {
            if (String.IsNullOrEmpty(eventName))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var guid = Guid.NewGuid().ToString();
            var queue = SharedAlertQueues.GetQueueByName(eventName);
            queue.RegisterInterest(guid);
            Session[guid] = queue;

            return Json(guid);
        }

        [HttpPost]
        public ActionResult RequestUpdateForUser(string guid)
        {
            if (String.IsNullOrEmpty(guid))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var queue = Session[guid] as ISharedAlertQueue<IUserUpdateData>;

            if (queue == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            return Json(queue.GetUpdates(guid, i => i.UserID == this.GetUserId()));
        }
    }
}