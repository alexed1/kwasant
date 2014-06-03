using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Threading.Tasks;
using Data.Entities.Enumerations;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Net.Mail;
using System.Net;
using System.Configuration;
using Data.Entities;
using Data.Interfaces;
using Data.Infrastructure;
using StructureMap;
using Utilities;
using KwasantWeb.ViewModels;
using KwasantCore.Services;
using KwasantWeb.Controllers.Helpers;
using ViewModel.Models;

namespace KwasantWeb.Controllers
{
    /// <summary>
    /// Email service
    /// </summary>
    public class KwasantEmailService : IIdentityMessageService
    {
        public async Task SendAsync(IdentityMessage message)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                String senderMailAddress = ConfigurationManager.AppSettings["fromEmail"];

                EmailDO emailDO = new EmailDO();
                emailDO.AddEmailRecipient(EmailParticipantType.TO, Email.GenerateEmailAddress(uow, new MailAddress(message.Destination)));
                emailDO.From = Email.GenerateEmailAddress(uow, new MailAddress(senderMailAddress));

                emailDO.Subject = message.Subject;
                emailDO.HTMLText = message.Body;

                Email userEmail = new Email(uow, emailDO);
                userEmail.Send();
            }
        }
    }

    [Authorize]
    public class AccountController : Controller
    {
        private UserManager<UserDO> GetUserManager(IUnitOfWork uow)
        {
            var um = new UserManager<UserDO>(new UserStore<UserDO>(uow.Db));

            var provider = new Microsoft.Owin.Security.DataProtection.DpapiDataProtectionProvider("Sample");
            um.UserTokenProvider = new Microsoft.AspNet.Identity.Owin.DataProtectorTokenProvider<UserDO>(provider.Create("EmailConfirmation"));
            return um;
        }
            
            
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult RegistrationSuccessful()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult MyAccount()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult LogOff()
        {
            new User().LogOff();
            return RedirectToAction("Login", "Account");
        }

        [AllowAnonymous]
        public ActionResult Confirm(RegisterViewModel model)
        {
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public  ActionResult Register(RegisterViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    RegistrationStatus curRegStatus = new Account().Register(model.Email.Trim(), model.Password.Trim());
                    if (curRegStatus == RegistrationStatus.UserMustLogIn)
                    {
                        ModelState.AddModelError("", "You are already registered with us. Please login.");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            catch (ApplicationException appEx)
            {
                ModelState.AddModelError("", appEx.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    LoginStatus curLoginStatus = await new Account().Login(model.Email.Trim(), model.Password, model.RememberMe);
                    switch (curLoginStatus)
                    {
                        case LoginStatus.InvalidCredential:
                            ModelState.AddModelError("", "Invalid Email id or Password.");
                            break;

                        case LoginStatus.ImplicitUser:
                            ModelState.AddModelError("", "We already have a record of that email address, but No password exists for this Email id. \nPlease register first.");
                            break;

                        case LoginStatus.UnregisteredUser:
                            ModelState.AddModelError("", "We do not have a registered account associated with this email address. \nPlease register first.");
                            break;

                        default:
                            if (curLoginStatus == LoginStatus.Successful)
                            {
                                //return Redirect(!String.IsNullOrEmpty(returnUrl) ? returnUrl : "/index.aspx");
                                //RedirectedToHomePage();
                                return RedirectToAction("Index", "Admin");
                            }
                            break;
                    }
                }
            }
            catch (ApplicationException appEx)
            {
                ModelState.AddModelError("", appEx.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        /// <summary>
        /// Send email for confirmation
        /// </summary>
        /// <param name="curUserDO"></param>
        private async Task SendEmailConfirmation(UserDO curUserDO)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var um = GetUserManager(uow);
                string code = await um.GenerateEmailConfirmationTokenAsync(curUserDO.Id);
                var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = curUserDO.Id, code = code }, protocol: Request.Url.Scheme);
                um.EmailService = new KwasantEmailService();
                await um.SendEmailAsync(curUserDO.Id, "Confirm your account", "Please confirm your account by clicking this link: <a href=\"" + callbackUrl + "\">Click here</a>");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ConfirmEmail(string userId, string code)
        {
            string returnViewName = "RegistrationSuccessful";
            try
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    UserDO curUserDO = uow.UserRepository.FindOne(u => u.Id == userId);
                    if (curUserDO != null)
                    {
                        curUserDO.EmailConfirmed = true;
                        uow.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                returnViewName = "Register";
            }

            return RedirectToAction(returnViewName);
        }


        public ActionResult Edit(String userId, String roleId)
        {
            if (String.IsNullOrEmpty(userId) || String.IsNullOrEmpty(roleId))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            UsersAdmin currUsersAdmin = new UsersAdmin();
            List<UsersAdminData> currUsersAdminDataList = currUsersAdmin.GetUsersAdminViewData(userId, roleId);

            List<UsersAdminViewModel> currUsersAdminViewModels = currUsersAdminDataList != null && currUsersAdminDataList.Count > 0 ? ObjectMapper.GetMappedUsersAdminViewModelList(currUsersAdminDataList) : null;

            UsersAdminViewModel currUsersAdminViewModel = currUsersAdminViewModels == null || currUsersAdminViewModels.Count == 0 ? new UsersAdminViewModel() : currUsersAdminViewModels[0];

            return View(currUsersAdminViewModel);
        }

        [HttpPost]
        public ActionResult Edit(UsersAdminViewModel usersAdminViewModel)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Check if any field edited by user on the font-end.
                if (!IsDirty(usersAdminViewModel))
                    return RedirectToAction("Index", "User");

                UserDO userDO = new UserDO();
                userDO.Id = usersAdminViewModel.UserId;
                userDO.FirstName = usersAdminViewModel.FirstName;
                userDO.LastName = usersAdminViewModel.LastName;
                userDO.EmailAddress = new EmailAddressDO()
                {
                    Id = usersAdminViewModel.EmailAddressID,
                    Address = usersAdminViewModel.EmailAddress
                };

                userDO.EmailAddressID = usersAdminViewModel.EmailAddressID;
                userDO.UserName = usersAdminViewModel.EmailAddress;

                IdentityUserRole identityUserRole = null;

                // Set RoleId & UserId if role is changed on the font-end other wise IdentityUserRole is set to null and user's role will not be updated.
                if (usersAdminViewModel.RoleId != usersAdminViewModel.PreviousRoleId)
                {
                    identityUserRole = new IdentityUserRole();
                    identityUserRole.RoleId = usersAdminViewModel.RoleId;
                    identityUserRole.UserId = usersAdminViewModel.UserId;

                    User user = new User();
                    user.ChangeUserRole(uow, identityUserRole);
                }

                uow.SaveChanges();
                return RedirectToAction("Index", "User");
            }
        }

        private bool IsDirty(UsersAdminViewModel usersAdminViewModel)
        {
            bool blnIsDirty = false;

            blnIsDirty = usersAdminViewModel.FirstName != usersAdminViewModel.PreviousFirstName ? true : usersAdminViewModel.LastName != usersAdminViewModel.PreviousLasttName ? true : usersAdminViewModel.FirstName != usersAdminViewModel.PreviousFirstName ? true : usersAdminViewModel.LastName != usersAdminViewModel.PreviousLasttName ? true : usersAdminViewModel.EmailAddress != usersAdminViewModel.PreviousEmailAddress ? true : usersAdminViewModel.RoleId != usersAdminViewModel.PreviousRoleId ? true : false;

            return blnIsDirty;
        }
    }
}