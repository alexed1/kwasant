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
using StructureMap;
using KwasantCore.Services;
using Data.Validations;
using System.Linq;
using Utilities;

namespace KwasantWeb.Controllers
{
    [KwasantAuthorize]
    public class UserController : Controller
    {
        private JsonPackager _jsonPackager;

        public UserController()
        {
            _jsonPackager = new JsonPackager();
        }

        [KwasantAuthorize(Roles = "Admin")]
        public ActionResult Index()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                List<UserDO> userList = uow.UserRepository.GetAll().ToList();
                UserShowAllVM userShowAllVM = new UserShowAllVM();
                List<UserShowVM> userShowVMList = new List<UserShowVM>();

                userList.ForEach(u => userShowVMList.Add(new UserShowVM
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    EmailAddress = u.EmailAddress.Address,
                    Role = uow.AspNetUserRolesRepository.GetRoles(u.Id).Select(r => r.Name).FirstOrDefault(),
                    RoleId = uow.AspNetUserRolesRepository.GetRoles(u.Id).Select(r => r.Id.ToString()).FirstOrDefault(),
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

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                UserDO curUser = uow.UserRepository.GetByKey(userId);
                UserShowVM userShowVM = new UserShowVM
                {
                    Id = curUser.Id,
                    FirstName = curUser.FirstName,
                    LastName = curUser.LastName,
                    EmailAddress = curUser.EmailAddress.Address,
                    Role = uow.AspNetUserRolesRepository.GetRoles(curUser.Id).Select(r => r.Name).FirstOrDefault(),
                    RoleId =
                        uow.AspNetUserRolesRepository.GetRoles(curUser.Id).Select(r => r.Id.ToString()).FirstOrDefault(),
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
                return RedirectToAction("MyAccount", new {remoteCalendarAccessGranted = providerName});
            }
            return new RedirectResult(result.RedirectUri);
        }

        public async Task<ActionResult> RevokeRemoteCalendarAccess(string providerName)
        {
            var authorizer = ObjectFactory.GetNamedInstance<IOAuthAuthorizer>(providerName);
            await authorizer.RevokeAccessTokenAsync(this.GetUserId(), CancellationToken.None);
            return RedirectToAction("MyAccount", new {remoteCalendarAccessForbidden = providerName});
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

        public ActionResult MyAccount(string remoteCalendarAccessGranted = null,
            string remoteCalendarAccessForbidden = null)
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

                var curManageUserVM =
                    AutoMapper.Mapper.Map<Tuple<UserDO, IEnumerable<RemoteCalendarProviderDO>>, ManageUserVM>(tuple);
                return View(curManageUserVM);
            }
        }

        public ActionResult ShowAddUser()
        {
            return View((UserDO) null);
        }

        public ActionResult Validate(UserCreateVM curCreateUserVM)
        {
            UserDO curUser = curCreateUserVM.User;
            string selectedRole = curCreateUserVM.UserRole;

            if (string.IsNullOrEmpty(curUser.EmailAddress.Address) || string.IsNullOrEmpty(curUser.FirstName) ||
                string.IsNullOrEmpty(curUser.LastName) || string.IsNullOrEmpty(selectedRole))
            {
                var jsonErrorResult = Json(_jsonPackager.Pack(new {Error = "All Fields are required"}),
                    JsonRequestBehavior.AllowGet);
                return jsonErrorResult;
            }

            EmailAddressValidator emailAddressValidator = new EmailAddressValidator();
            if (!(emailAddressValidator.Validate(curUser.EmailAddress).IsValid))
            {
                var jsonErrorResult = Json(_jsonPackager.Pack(new {Error = "Please provide valid email address"}),
                    JsonRequestBehavior.AllowGet);
                return jsonErrorResult;
            }
            var jsonSuccessResult = Json(_jsonPackager.Pack("valid"), JsonRequestBehavior.AllowGet);
            return jsonSuccessResult;
        }

        [KwasantAuthorize(Roles = "Admin")]
        public ActionResult Detail(String userId)
        {
            UserAdministerVM curUserAdminVM = new UserAdministerVM();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                curUserAdminVM.User = uow.UserRepository.GetByKey(userId);
                curUserAdminVM.Role = uow.AspNetUserRolesRepository.GetRoles(userId).Select(r => r.Name).FirstOrDefault();
                return View(curUserAdminVM);
            }
        }

        [KwasantAuthorize(Roles = "Admin")]
        public ActionResult FindUser(UserAdministerVM curUserAdminVM) //This wasn't even searching before...???
        {
            return View(new UserAdministerVM());
        }

        [HttpPost]
        public ActionResult RunQuery(UserQueryVM curUserQueryVM)
        {
            UserDO queryParams = curUserQueryVM.User;

            if (string.IsNullOrEmpty(queryParams.EmailAddress.Address) && string.IsNullOrEmpty(queryParams.FirstName) &&
                string.IsNullOrEmpty(queryParams.LastName))
            {
                var jsonErrorResult = Json(_jsonPackager.Pack(new {Error = "Atleast one field is required"}),
                    JsonRequestBehavior.AllowGet);
                return jsonErrorResult;
            }
            if (queryParams.EmailAddress.Address != null)
            {
                EmailAddressValidator emailAddressValidator = new EmailAddressValidator();
                if (!(emailAddressValidator.Validate(queryParams.EmailAddress).IsValid))
                {
                    var jsonErrorResult = Json(_jsonPackager.Pack(new {Error = "Please provide valid email address"}),
                        JsonRequestBehavior.AllowGet);
                    return jsonErrorResult;
                }
            }
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var query = uow.UserRepository.GetQuery();
                if (!String.IsNullOrWhiteSpace(queryParams.FirstName))
                    query = query.Where(u => u.FirstName.Contains(queryParams.FirstName));
                if (!String.IsNullOrWhiteSpace(queryParams.LastName))
                    query = query.Where(u => u.LastName.Contains(queryParams.LastName));
                if (!String.IsNullOrWhiteSpace(queryParams.EmailAddress.Address))
                    query = query.Where(u => u.EmailAddress.Address.Contains(queryParams.EmailAddress.Address));

                var matchedUsers = query.ToList();

                var jsonResult = Json(_jsonPackager.Pack(matchedUsers), JsonRequestBehavior.AllowGet);
                // var jsonResult = Json(_jsonPackager.Pack(_user.Query(uow, queryParams)), JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }

        [HttpPost]
        public ActionResult ProcessSubmittedForm(UserCreateVM curCreateUserVM, bool sendEmail)
        {
            UserDO curUser = curCreateUserVM.User;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var matchingRole =
                    uow.AspNetRolesRepository.GetQuery().FirstOrDefault(e => e.Id == curCreateUserVM.UserRole);
                if (matchingRole != null)
                {
                    string role = matchingRole.Name;

                    if (sendEmail)
                        new Email().SendUserSettingsNotification(uow, curUser);

                    var newUser = uow.UserRepository.GetOrCreateUser(curUser.EmailAddress.Address);
                    uow.UserRepository.UpdateUserCredentials(newUser, curUser.UserName, Guid.NewGuid().ToString());
                    uow.AspNetUserRolesRepository.AssignRoleToUser(role, newUser.Id);
                    uow.SaveChanges();
                }
                return RedirectToAction("Dashboard", "Admin");
            }
        }

        [HttpPost]
        public ActionResult Update(UserCreateVM curCreateUserVM)
        {
            UserDO curUser = curCreateUserVM.User;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                string role = uow.AspNetRolesRepository.GetQuery().FirstOrDefault(r => r.Id == curCreateUserVM.UserRole).Name;
                var existingUser = uow.UserRepository.GetOrCreateUser(curUser.EmailAddress.Address);
                uow.UserRepository.UpdateUserCredentials(existingUser, curUser.UserName, Guid.NewGuid().ToString());
                uow.AspNetUserRolesRepository.AssignRoleToUser(role, existingUser.Id);
            }
            var jsonSuccessResult = Json(_jsonPackager.Pack("User updated successfully."), JsonRequestBehavior.AllowGet);
            return jsonSuccessResult;
        }

    }
}