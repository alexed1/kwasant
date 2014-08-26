using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Net;
using Data.Entities;
using Data.Interfaces;
using KwasantCore.Exceptions;
using KwasantCore.Managers;
using KwasantCore.Managers.APIManagers.Authorizers;
using KwasantWeb.ViewModels;
using Microsoft.AspNet.Identity;
using StructureMap;
using ViewModel.Models;
using KwasantCore.Services;
using KwasantWeb.Controllers.Helpers;
using KwasantCore.Managers.APIManager.Packagers.DataTable;
using System.Linq;
using Data.Validators;

namespace KwasantWeb.Controllers
{
    [KwasantAuthorize(Roles = "Customer")]
    public class UserController : Controller
    {
        private DataTablesPackager _datatables;
        private User _user;

        public UserController()
        {
            _datatables = new DataTablesPackager();
            _user = new User();
        }

        [KwasantAuthorize(Roles = "Admin")]
        public ActionResult Index()
        {
            UsersAdmin currUsersAdmin = new UsersAdmin();
            List<UsersAdminData> currUsersAdminDataList = currUsersAdmin.GetUsersAdminViewData();
            List<UsersAdminVM> currUsersAdminVMs = currUsersAdminDataList != null && currUsersAdminDataList.Count > 0 ? ObjectMapper.GetMappedUsersAdminVMList(currUsersAdminDataList) : null;

            return View(currUsersAdminVMs);
        }

        [KwasantAuthorize(Roles = "Admin")]
        public ActionResult Details(String userId, String roleId)
        {

            if (String.IsNullOrEmpty(userId) || String.IsNullOrEmpty(roleId))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            UsersAdmin currUsersAdmin = new UsersAdmin();
            List<UsersAdminData> currUsersAdminDataList = currUsersAdmin.GetUsersAdminViewData(userId, roleId);

            List<UsersAdminVM> currUsersAdminVMs = currUsersAdminDataList != null && currUsersAdminDataList.Count > 0 ? ObjectMapper.GetMappedUsersAdminVMList(currUsersAdminDataList) : null;

            UsersAdminVM currUsersAdminVM = currUsersAdminVMs == null || currUsersAdminVMs.Count == 0 ? new UsersAdminVM() : currUsersAdminVMs[0];

            return View(currUsersAdminVM);
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

        [KwasantAuthorize(Roles = "Admin")]
        public ActionResult ShowUserQueryForm(UserAdministerVM curUserAdminVM)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                curUserAdminVM.User = new UserDO
                {
                    Id = "",
                    EmailAddress = new EmailAddressDO() { Address = "" },
                    FirstName = "",
                    LastName = ""
                };
            }
            return View("Query", curUserAdminVM);
        }

        [HttpPost]
        public ActionResult FindUser(UserQueryVM curUserQueryVM)
        {
            UserDO curUser = curUserQueryVM.User;

            if (string.IsNullOrEmpty(curUser.EmailAddress.Address) && string.IsNullOrEmpty(curUser.FirstName) && string.IsNullOrEmpty(curUser.LastName)) 
            {
                var jsonErrorResult = Json(_datatables.Pack(new { Error = "Atleast one field is required" }), JsonRequestBehavior.AllowGet);
                return jsonErrorResult;
            }
            if (curUser.EmailAddress.Address != null)
            {
                EmailAddressValidator emailAddressValidator = new EmailAddressValidator();
                if (!(emailAddressValidator.Validate(curUser.EmailAddress).IsValid))
                {
                    var jsonErrorResult = Json(_datatables.Pack(new { Error = "Please provide valid email address" }), JsonRequestBehavior.AllowGet);
                    return jsonErrorResult;
                }
            }
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var jsonResult = Json(_datatables.Pack(_user.Query(uow, curUser)), JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }

        public ActionResult GetCalendars(string curUserId)
        {
            UserAdministerVM curUserAdminVM = new UserAdministerVM();
            curUserAdminVM.User = _user.GetUser(curUserId);
            return PartialView("ShowCalendars", curUserAdminVM);

        }

        public ActionResult ChangeUser(string curUserId)
        {
            UserQueryVM curUserQueryVM = new UserQueryVM();
            curUserQueryVM.User = _user.GetUser(curUserId);
            return PartialView("ShowQuery", curUserQueryVM);
        }

        public ActionResult ShowAddUser()
        {
            UserAdministerVM curUserAdminVM = new UserAdministerVM();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                curUserAdminVM.User = new UserDO
                {
                    Id = "",
                    EmailAddress = new EmailAddressDO() { Address = "" },
                    FirstName = "",
                    LastName = ""
                };
            }
            return View(curUserAdminVM);
        }

        public ActionResult Edit(CreateUserVM curCreateUserVM)
        {
            UserDO curUser = curCreateUserVM.User;
            string selectedRole = curCreateUserVM.UserRole;
            
            if (string.IsNullOrEmpty(curUser.EmailAddress.Address) || string.IsNullOrEmpty(curUser.FirstName) || string.IsNullOrEmpty(curUser.LastName) || string.IsNullOrEmpty(selectedRole))
            {
                var jsonErrorResult = Json(_datatables.Pack(new { Error = "All Fields are required" }), JsonRequestBehavior.AllowGet);
                return jsonErrorResult;
            }

            EmailAddressValidator emailAddressValidator = new EmailAddressValidator();
            if (!(emailAddressValidator.Validate(curUser.EmailAddress).IsValid))
            {
                var jsonErrorResult = Json(_datatables.Pack(new { Error = "Please provide valid email address" }), JsonRequestBehavior.AllowGet);
                return jsonErrorResult;
            }
            var jsonSuccessResult = Json(_datatables.Pack("valid"), JsonRequestBehavior.AllowGet);
            return jsonSuccessResult;
        }

        public ActionResult ShowDetails(string curUserId)
        {
            CreateUserVM curUserAdminVM = new CreateUserVM();
            curUserAdminVM.User = _user.GetUser(curUserId);
            curUserAdminVM.UserRole = _user.GetRole(curUserId);
            return PartialView("ShowDetails", curUserAdminVM);
        }

        public ActionResult ProcessEdits(CreateUserVM curCreateUserVM, bool sendEmail)
        {
            UserDO curUser = curCreateUserVM.User;
            string role = curCreateUserVM.Roles.Where(e => e.Id == curCreateUserVM.UserRole).FirstOrDefault().Name;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                _user.CreateOrUpdateUser(uow, curUser, role, sendEmail);
            }
            var jsonSuccessResult = Json(_datatables.Pack("User updated successfully."), JsonRequestBehavior.AllowGet);
            return jsonSuccessResult;
        }
    }
}

