using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using Data.Entities;
using Data.Interfaces;
using KwasantCore.Managers;
using KwasantCore.Managers.APIManagers.Authorizers;
using KwasantWeb.ViewModels;
using Microsoft.Ajax.Utilities;
using StructureMap;
using Data.Validations;
using System.Linq;
using Utilities;

namespace KwasantWeb.Controllers
{
    [KwasantAuthorize]
    public class UserController : Controller
    {
        private readonly JsonPackager _jsonPackager;

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
                
                var userVMList = userList.Select(u => CreateUserVM(u, uow)).ToList();

                return View(userVMList);
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

        [KwasantAuthorize(Roles = "Admin")]
        public ActionResult Details(String userId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var userDO = uow.UserRepository.GetByKey(userId);
                var userVM = CreateUserVM(userDO, uow);

                return View(userVM);
            }
        }


        [HttpPost]
        public ActionResult RunQuery(UserVM queryParams)
        {
            if (string.IsNullOrEmpty(queryParams.EmailAddress) && string.IsNullOrEmpty(queryParams.FirstName) &&
                string.IsNullOrEmpty(queryParams.LastName))
            {
                var jsonErrorResult = Json(_jsonPackager.Pack(new {Error = "Atleast one field is required"}),
                    JsonRequestBehavior.AllowGet);
                return jsonErrorResult;
            }
            if (queryParams.EmailAddress != null)
            {
                EmailAddressValidator emailAddressValidator = new EmailAddressValidator();
                if (!(emailAddressValidator.Validate(new EmailAddressDO(queryParams.EmailAddress)).IsValid))
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
                if (!String.IsNullOrWhiteSpace(queryParams.EmailAddress))
                    query = query.Where(u => u.EmailAddress.Address.Contains(queryParams.EmailAddress));

                var matchedUsers = query.ToList();

                var jsonResult = Json(_jsonPackager.Pack(matchedUsers), JsonRequestBehavior.AllowGet);

                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }

        [HttpPost]
        public ActionResult Update(UserVM curCreateUserVM)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var existingUser = uow.UserRepository.GetOrCreateUser(curCreateUserVM.EmailAddress);
                uow.UserRepository.UpdateUserCredentials(existingUser, curCreateUserVM.UserName,
                    Guid.NewGuid().ToString());

                var existingRoles = uow.AspNetUserRolesRepository.GetRoles(existingUser.Id).ToList();

                //Remove old roles
                foreach (var existingRole in existingRoles)
                {
                    if (!curCreateUserVM.Roles.Select(newRole => newRole).Contains(existingRole.Name))
                        uow.AspNetUserRolesRepository.RevokeRoleFromUser(existingRole.Name, existingUser.Id);
                }

                //Add new roles
                foreach (var role in curCreateUserVM.Roles)
                {
                    if (!existingRoles.Select(newRole => newRole.Name).Contains(role))
                        uow.AspNetUserRolesRepository.AssignRoleToUser(role, existingUser.Id);    
                }

                existingUser.FirstName = curCreateUserVM.FirstName;
                existingUser.LastName = curCreateUserVM.LastName;
                existingUser.EmailAddress = uow.EmailAddressRepository.GetOrCreateEmailAddress(curCreateUserVM.EmailAddress, curCreateUserVM.FirstName);
                uow.SaveChanges();
            }
            var jsonSuccessResult = Json(_jsonPackager.Pack("User updated successfully."), JsonRequestBehavior.AllowGet);
            return jsonSuccessResult;
        }

        public ActionResult FindUser()
        {
            return View();
        }

        public ActionResult Search(String firstName, String lastName, String emailAddress)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var users = uow.UserRepository.GetQuery();
                if (!String.IsNullOrWhiteSpace(firstName))
                    users = users.Where(u => u.FirstName.Contains(firstName));
                if (!String.IsNullOrWhiteSpace(lastName))
                    users = users.Where(u => u.LastName.Contains(lastName));
                if (!String.IsNullOrWhiteSpace(emailAddress))
                    users = users.Where(u => u.EmailAddress.Address.Contains(emailAddress));

                return new JsonResult
                {
                    Data = users.ToList().Select(u => new
                    {
                        Id = u.Id,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        EmailAddress = u.EmailAddress.Address
                    }).ToList()
                };
            }
        }

        private static UserVM CreateUserVM(UserDO u, IUnitOfWork uow)
        {
            return new UserVM
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                UserName = u.UserName,
                EmailAddress = u.EmailAddress.Address,
                Roles = uow.AspNetUserRolesRepository.GetRoles(u.Id).Select(r => r.Name).ToList(),
                Calendars = u.Calendars.Select(c => new UserCalendarVM { Id = c.Id, Name = c.Name}).ToList()
            };
        }

    }
}