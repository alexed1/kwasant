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
using System.Linq;

namespace KwasantWeb.Controllers
{
    [KwasantAuthorize(Roles = "Customer")]
    public class UserController : Controller
    {
        [KwasantAuthorize(Roles = "Admin")]
        public ActionResult Index()
        {
            
            string userId = "";
            User _user = new User();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                List<UserDO> userList = _user.Query(uow, userId);
                UserShowAllVM userShowAllVM = new UserShowAllVM();
                List<UserShowVM> userShowVMList = new List<UserShowVM>();

                var userManager = KwasantCore.Services.User.GetUserManager(uow);
                userList.ForEach(u => userShowVMList.Add(new UserShowVM
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    EmailAddress = u.EmailAddress.Address,
                    Role = userManager.GetRoles(u.Id)[0],
                    RoleId = u.Roles.ToList()[0].RoleId
                }));
                userShowAllVM.Users = userShowVMList;
                return View(userShowAllVM);
            }
        }

        [KwasantAuthorize(Roles = "Admin")]
        public ActionResult Details(String userId, String roleId)
        {

            if (String.IsNullOrEmpty(userId) || String.IsNullOrEmpty(roleId))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            User _user = new User();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curUserManager = KwasantCore.Services.User.GetUserManager(uow);
                UserDO curUser = _user.Query(uow, userId)[0];
                UserShowVM userShowVM = new UserShowVM
                {
                    Id = curUser.Id,
                    FirstName = curUser.FirstName,
                    LastName = curUser.LastName,
                    EmailAddress = curUser.EmailAddress.Address,
                    Role = curUserManager.GetRoles(curUser.Id)[0],
                    RoleId = curUser.Roles.ToList()[0].RoleId
                };
                return View(userShowVM);
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
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
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

