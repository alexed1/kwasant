using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Data.Interfaces;
using KwasantCore.Managers;
using KwasantWeb.AlertQueues;
using StructureMap;

namespace KwasantWeb.Controllers
{
    [KwasantAuthorize]
    public class AlertingController : Controller
    {
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
            
            return new JsonResult { Data = guid };
        }

        public ActionResult RequestUpdate(string guid)
        {
            if (String.IsNullOrEmpty(guid))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var queue = Session[guid] as IPersonalAlertQueue;

            if (queue == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            return new JsonResult { Data = queue.GetUpdates() };
        }

        public ActionResult RegisterInterestInUserUpdates(string eventName)
        {
            if (String.IsNullOrEmpty(eventName))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var guid = Guid.NewGuid().ToString();
            var queue = (ISharedAlertQueue<IUserUpdateData>)SharedAlertQueues.GetQueueByName(eventName);
            queue.RegisterInterest(guid);
            Session[guid] = queue;

            return new JsonResult { Data = guid };
        }

        public ActionResult RequestUpdateForUser(string guid)
        {
            if (String.IsNullOrEmpty(guid))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var queue = Session[guid] as ISharedAlertQueue<IUserUpdateData>;

            if (queue == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            return new JsonResult { Data = queue.GetUpdates(guid, i => i.UserID == this.GetUserId()) };
        }

        public ActionResult RegisterInterestInRoleUpdates(string eventName)
        {
            if (String.IsNullOrEmpty(eventName))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var guid = Guid.NewGuid().ToString();
            var queue = (ISharedAlertQueue<IRoleUpdateData>)SharedAlertQueues.GetQueueByName(eventName);
            queue.RegisterInterest(guid);
            Session[guid] = queue;

            return new JsonResult { Data = guid };
        }
        
        [HttpPost]
        public ActionResult RequestUpdateForRole(string guid)
        {
            if (String.IsNullOrEmpty(guid))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var queue = Session[guid] as ISharedAlertQueue<IRoleUpdateData>;

            if (queue == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var roles = this.GetRoleNames();
            return Json(queue.GetUpdates(guid, i => i.RoleNames.Any(roles.Contains)));
        }
    }
}