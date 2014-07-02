using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Net;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Managers.APIManager.Authorizers.Google;
using KwasantCore.Managers.IdentityManager;
using KwasantWeb.ViewModels;
using Microsoft.AspNet.Identity;
using StructureMap;
using ViewModel.Models;
using KwasantCore.Services;
using KwasantWeb.Controllers.Helpers;

namespace KwasantWeb.Controllers
{
    [KwasantAuthorize(Roles = "Customer")]
    public class UserController : Controller
    {
        [KwasantAuthorize(Roles = "Admin")]
        public ActionResult Index()
        {
            UsersAdmin currUsersAdmin = new UsersAdmin();
            List<UsersAdminData> currUsersAdminDataList = currUsersAdmin.GetUsersAdminViewData();
            List<UsersAdminViewModel> currUsersAdminViewModels = currUsersAdminDataList != null && currUsersAdminDataList.Count > 0 ? ObjectMapper.GetMappedUsersAdminViewModelList(currUsersAdminDataList) : null;

            return View(currUsersAdminViewModels);
        }

        [KwasantAuthorize(Roles = "Admin")]
        public ActionResult Details(String userId, String roleId)
        {

            if (String.IsNullOrEmpty(userId) || String.IsNullOrEmpty(roleId))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            UsersAdmin currUsersAdmin = new UsersAdmin();
            List<UsersAdminData> currUsersAdminDataList = currUsersAdmin.GetUsersAdminViewData(userId, roleId);

            List<UsersAdminViewModel> currUsersAdminViewModels = currUsersAdminDataList != null && currUsersAdminDataList.Count > 0 ? ObjectMapper.GetMappedUsersAdminViewModelList(currUsersAdminDataList) : null;

            UsersAdminViewModel currUsersAdminViewModel = currUsersAdminViewModels == null || currUsersAdminViewModels.Count == 0 ? new UsersAdminViewModel() : currUsersAdminViewModels[0];

            return View(currUsersAdminViewModel);
        }

        public async Task<ActionResult> GrantGoogleCalendarAccess()
        {
            var authorizer = new GoogleCalendarAuthorizer(this.GetUserId(),
                                                          this.User.Identity.GetUserName());
            var result = await authorizer.AuthorizeAsync(
                Url.Action("IndexAsync", "AuthCallback", null, this.Request.Url.Scheme),
                Request.RawUrl,
                CancellationToken.None);

            if (result.Credential != null)
            {
                return RedirectToAction("MyAccount", new { googleCalendarAccessGranted = true });
            }
            else
            {
                return new RedirectResult(result.RedirectUri);
            }
        }

        public async Task<ActionResult> RevokeGoogleCalendarAccess()
        {
            var authorizer = new GoogleCalendarAuthorizer(this.GetUserId());
            await authorizer.RevokeAccessTokenAsync(CancellationToken.None);
            return RedirectToAction("MyAccount", new { googleCalendarAccessForbidden = true });
        }

        public ActionResult MyAccount(bool googleCalendarAccessGranted = false, bool googleCalendarAccessForbidden = false)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curUserDO = uow.UserRepository.GetByKey(this.GetUserId());
                var curManageUserViewModel = AutoMapper.Mapper.Map<UserDO, ManageUserViewModel>(curUserDO);

                if (googleCalendarAccessGranted)
                {
                    ViewBag.StatusMessage = "Google Calendar hooked up successfully.";
                }
                else if (googleCalendarAccessForbidden)
                {
                    ViewBag.StatusMessage = "Google Calendar access revoked.";
                }
                return View(curManageUserViewModel);
            }
        }

    }
}

