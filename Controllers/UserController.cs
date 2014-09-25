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
using Data.Validations;
using System.Linq;
using KwasantCore.Managers.APIManager.Packagers.DataTable;

namespace KwasantWeb.Controllers
{
    [KwasantAuthorize]
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
            var result = await authorizer.AuthorizeAsync(
                this.GetUserId(),
                this.GetUserName(),
                String.Format("{0}AuthCallback/IndexAsync", Utilities.Server.ServerUrl),
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

        public ActionResult Validate(UserCreateVM curCreateUserVM)
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

        [KwasantAuthorize(Roles = "Admin")]
        public ActionResult Detail(String userId)
        {
            UserAdministerVM curUserAdminVM = new UserAdministerVM();
            curUserAdminVM.User = _user.Get(userId);
            return View(curUserAdminVM);
        }

        [KwasantAuthorize(Roles = "Admin")]
        public ActionResult FindUser(UserAdministerVM curUserAdminVM)
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
            return View(curUserAdminVM);
        }

        [HttpPost]
        public ActionResult RunQuery(UserQueryVM curUserQueryVM)
        {
            UserDO queryParams = curUserQueryVM.User;

            if (string.IsNullOrEmpty(queryParams.EmailAddress.Address) && string.IsNullOrEmpty(queryParams.FirstName) && string.IsNullOrEmpty(queryParams.LastName))
            {
                var jsonErrorResult = Json(_datatables.Pack(new { Error = "Atleast one field is required" }), JsonRequestBehavior.AllowGet);
                return jsonErrorResult;
            }
            if (queryParams.EmailAddress.Address != null)
            {
                EmailAddressValidator emailAddressValidator = new EmailAddressValidator();
                if (!(emailAddressValidator.Validate(queryParams.EmailAddress).IsValid))
                {
                    var jsonErrorResult = Json(_datatables.Pack(new { Error = "Please provide valid email address" }), JsonRequestBehavior.AllowGet);
                    return jsonErrorResult;
                }
            }
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var jsonResult = Json(_datatables.Pack(_user.Query(uow, queryParams)), JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }

        [HttpPost]
        public ActionResult ProcessSubmittedForm(UserCreateVM curCreateUserVM, bool sendEmail)
        {
            UserDO curUser = curCreateUserVM.User;
            string role = new Role().GetRoles().Where(e => e.Id == curCreateUserVM.UserRole).FirstOrDefault().Name;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                _user.Create(uow, curUser, role, sendEmail);
                uow.SaveChanges();
            }
            return RedirectToAction("Dashboard", "Admin");
        }

        [HttpPost]
        public ActionResult Update(UserCreateVM curCreateUserVM)
        {
            UserDO curUser = curCreateUserVM.User;
            string role = new Role().GetRoles().Where(e => e.Id == curCreateUserVM.UserRole).FirstOrDefault().Name;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                _user.Update(uow, curUser, role);
            }
            var jsonSuccessResult = Json(_datatables.Pack("User updated successfully."), JsonRequestBehavior.AllowGet);
            return jsonSuccessResult;
        }

    }
}

