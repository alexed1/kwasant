using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Net;
using Data.Entities;
using Data.Interfaces;
using KwasantCore.Managers;
using KwasantCore.Managers.APIManagers.Authorizers;
using KwasantWeb.ViewModels;
using Microsoft.AspNet.Identity;
using StructureMap;
using ViewModel.Models;
using KwasantCore.Services;
using KwasantWeb.Controllers.Helpers;

using KwasantCore.Helper;

namespace KwasantWeb.Controllers
{
    [KwasantAuthorize(Roles = "Customer")]
    public class UserController : Controller
    {
        [KwasantAuthorize(Roles = "Admin")]
        public ActionResult Index()
        {
            string userId = "";
            User currUser = new User();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                List<UserVM> currUserVMs = new List<UserVM>();
                List<UserData> currUserDataList = currUser.Query(uow, userId);
                currUserDataList.ForEach(e => currUserVMs.Add(new UserVM { User = e.User, Role = e.Role, RoleId = e.RoleId }));
                return View(currUserVMs);
            }
        }

        [KwasantAuthorize(Roles = "Admin")]
        public ActionResult Details(String userId, String roleId)
        {

            if (String.IsNullOrEmpty(userId) || String.IsNullOrEmpty(roleId))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            User currUser = new User();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {

                UserData currUserData = currUser.Query(uow, userId)[0];
                UserVM currUserVM = new UserVM { User = currUserData.User, Role = currUserData.Role, RoleId = currUserData.RoleId };
                return View(currUserVM);
            }
        }

        public async Task<ActionResult> GrantRemoteCalendarAccess(string providerName)
        {
            var authorizer = ObjectFactory.GetNamedInstance<IOAuthAuthorizer>(providerName);
            var result = await authorizer.AuthorizeAsync(this.GetUserId(),
                                                         this.User.Identity.GetUserName(),
                                                         Url.Action("IndexAsync", "AuthCallback", null,
                                                                    this.Request.Url.Scheme),
                                                         Request.RawUrl,
                                                         CancellationToken.None);

            if (result.Credential != null)
            {
                // don't wait for this, run it async and return response to the user.
                return RedirectToAction("MyAccount", new { remoteCalendarAccessGranted = providerName });
            }
            else
            {
                return new RedirectResult(result.RedirectUri);
            }
        }

        public async Task<ActionResult> RevokeRemoteCalendarAccess(string providerName)
        {
            var authorizer = ObjectFactory.GetNamedInstance<IOAuthAuthorizer>(providerName);
            await authorizer.RevokeAccessTokenAsync(this.GetUserId(), CancellationToken.None);
            return RedirectToAction("MyAccount", new { remoteCalendarAccessForbidden = providerName });
        }

        public async Task<ActionResult> SyncCalendarsNow()
        {
            try
            {
                await ObjectFactory.GetInstance<CalendarSyncManager>().SyncNowAsync(this.GetUserId());
                return Json(new {success = true});
            }
            catch (Exception ex)
            {
                return Json(new {success = false, error = ex.Message});
            }
        }

        public ActionResult MyAccount(string remoteCalendarAccessGranted = null, string remoteCalendarAccessForbidden = null)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curUserId = this.GetUserId();
                var curUserDO = uow.UserRepository.GetByKey(curUserId);
                if (curUserDO == null)
                {
                    // if we found no user then assume that this user doesn't exists any more and force log off action.
                    return RedirectToAction("LogOff", "Account");
                }
                var remoteCalendarProviders = uow.RemoteCalendarProviderRepository.GetAll();
                var tuple = new Tuple<UserDO, IEnumerable<RemoteCalendarProviderDO>>(curUserDO, remoteCalendarProviders);
                
                var curManageUserVM = AutoMapper.Mapper.Map<Tuple<UserDO, IEnumerable<RemoteCalendarProviderDO>>, ManageUserVM>(tuple);
                return View(curManageUserVM);
            }
        }

    }
}

