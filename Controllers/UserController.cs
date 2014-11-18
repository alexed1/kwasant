using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using KwasantCore.Managers;
using KwasantCore.Managers.APIManagers.Authorizers;
using KwasantWeb.ViewModels;
using Microsoft.Ajax.Utilities;
using StructureMap;
using Data.Validations;
using System.Linq;
using Utilities;
using Data.Infrastructure;
using KwasantCore.Services;

namespace KwasantWeb.Controllers
{
    [KwasantAuthorize]
    public class UserController : Controller
    {
        private readonly JsonPackager _jsonPackager;
        private readonly User _user;
        public UserController()
        {
            _jsonPackager = new JsonPackager();
            _user = new User();
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
            return View(new UserVM());
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
                var jsonErrorResult = Json(_jsonPackager.Pack(new {Error = "Atleast one field is required"}));
                return jsonErrorResult;
            }
            if (queryParams.EmailAddress != null)
            {
                EmailAddressValidator emailAddressValidator = new EmailAddressValidator();
                if (!(emailAddressValidator.Validate(new EmailAddressDO(queryParams.EmailAddress)).IsValid))
                {
                    var jsonErrorResult = Json(_jsonPackager.Pack(new {Error = "Please provide valid email address"}));
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

                var jsonResult = Json(_jsonPackager.Pack(matchedUsers));

                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }

        [HttpPost]
        [KwasantAuthorize(Roles = Roles.Admin)]
        public ActionResult Update(UserVM curCreateUserVM)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                bool isAlreadyExists;
                UserDO existingUser = _user.GetUserToAddOrUpdate(uow, curCreateUserVM.Id, curCreateUserVM.EmailAddress, curCreateUserVM.FirstName, out isAlreadyExists);
                if (isAlreadyExists)
                {
                    var jsonSuccessResult = Json(_jsonPackager.Pack(new { Data = "User already exists.", UserId = existingUser.Id }));
                    return jsonSuccessResult;
                }

                if (!String.IsNullOrEmpty(curCreateUserVM.NewPassword))
                {
                    _user.UpdatePassword(uow, existingUser, curCreateUserVM.NewPassword);
                }

                _user.SetRoles(uow, existingUser.Id, curCreateUserVM.Roles);

                existingUser.FirstName = curCreateUserVM.FirstName;
                existingUser.LastName = curCreateUserVM.LastName;
                uow.SaveChanges();

                //Checking if user is new user
                if (String.IsNullOrWhiteSpace(curCreateUserVM.Id))
                {
                    AlertManager.ExplicitCustomerCreated(existingUser.Id);
                }
                //Sending a mail to user with newly created credentials if send email is checked
                if (curCreateUserVM.SendMail && !String.IsNullOrEmpty(curCreateUserVM.NewPassword))
                {
                    new Email().SendLoginCredentials(uow, curCreateUserVM.EmailAddress, curCreateUserVM.NewPassword);
                }
                return Json(_jsonPackager.Pack("User updated successfully."));
            }
        }

        public ActionResult FindUser()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Search(String firstName, String lastName, String emailAddress,int[] states)
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

                users = users.Where(u => states.Contains(u.State.Value));

                return Json(users.ToList().Select(u => new
                    {
                        Id = u.Id,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        EmailAddress = u.EmailAddress.Address
                    }).ToList()
                );
            }
        }

        private UserVM CreateUserVM(UserDO u, IUnitOfWork uow)
        {
            return new UserVM
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                UserName = u.UserName,
                EmailAddress = u.EmailAddress.Address,
                Roles = _user.GetSelectedRole(uow.AspNetUserRolesRepository.GetRoles(u.Id).Select(r => r.Name).ToArray()),
                Calendars = u.Calendars.Select(c => new UserCalendarVM { Id = c.Id, Name = c.Name }).ToList(),
                EmailAddressID = u.EmailAddressID.Value,
                Status = u.State.Value
            };
        }

        //Update User Status from user details view valid states are "Active" and "Deleted"
        public void UpdateStatus(string userId, int status)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                UserDO curUser = uow.UserRepository.GetQuery().Where(user => user.Id == userId).FirstOrDefault();

                if (curUser != null)
                {
                    curUser.State = status;
                    uow.SaveChanges();
                }
            }
        }

        public ActionResult ExistingUserAlert(string UserId)
        {
            ViewBag.UserId = UserId;
            return View();
        }

    }
}