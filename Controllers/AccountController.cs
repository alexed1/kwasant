
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using KwasantCore.Managers;
using KwasantCore.Services;
using KwasantWeb.Controllers.Helpers;
using KwasantWeb.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using StructureMap;
using Utilities;
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
                String senderMailAddress = ObjectFactory.GetInstance<IConfigRepository>().Get("EmailFromAddress_DirectMode");

                EmailDO emailDO = new EmailDO();
                emailDO.AddEmailRecipient(EmailParticipantType.To, Email.GenerateEmailAddress(uow, new MailAddress(message.Destination)));
                emailDO.From = Email.GenerateEmailAddress(uow, new MailAddress(senderMailAddress));

                emailDO.Subject = message.Subject;
                emailDO.HTMLText = message.Body;

                uow.EnvelopeRepository.ConfigurePlainEmail(emailDO);
                uow.SaveChanges();
            }
        }
    }

    [KwasantAuthorize]
    public class AccountController : Controller
    {
        [AllowAnonymous]
        public ActionResult InterceptLogin(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (this.UserIsAuthenticated())
                throw new HttpException(403, "You do not have access to view this page.");
            return View("Index");
        }

        [AllowAnonymous]
        public ActionResult Index(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (this.UserIsAuthenticated())
                return RedirectToAction("MyAccount", "User");
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
        public ActionResult LogOff()
        {
            this.Logout();
            return RedirectToAction("Index", "Account");
        }

        [AllowAnonymous]
        public ActionResult Confirm(RegisterVM model)
        {
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public  ActionResult Register(RegisterVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    RegistrationStatus curRegStatus = new Account().ProcessRegistrationRequest(model.Email.Trim(), model.Password.Trim());
                    if (curRegStatus == RegistrationStatus.UserMustLogIn)
                    {
                        ModelState.AddModelError("", "You are already registered with us. Please login.");
                    }
                    else
                    {
                       // return RedirectToAction("Index", "Home");
                        return View("RegistrationConfirmation");
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
        public async Task<ActionResult> Login(LoginVM model, string returnUrl)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    string username = model.Email.Trim();
                    LoginStatus curLoginStatus = await new Account().ProcessLoginRequest(username, model.Password, model.RememberMe);
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
                                if (!String.IsNullOrEmpty(returnUrl))
                                    return Redirect(returnUrl);

                                bool isAdmin;
                                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                                {
                                    var userManager = KwasantCore.Services.User.GetUserManager(uow);
                                    var user = userManager.FindByName(username);
                                    isAdmin = userManager.IsInRole(user.Id, "Admin");
                                }

                                if (isAdmin)
                                    return RedirectToAction("Index", "Admin");

                                return RedirectToAction("MyAccount", "User");
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
            return View("Index", model);
        }

        /// <summary>
        /// Send email for confirmation
        /// </summary>
        /// <param name="curUserDO"></param>
        private async Task SendEmailConfirmation(UserDO curUserDO)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var um = KwasantCore.Services.User.GetUserManager(uow);
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


        //public ActionResult Edit(String userId, String roleId)
        //{
        //    if (String.IsNullOrEmpty(userId) || String.IsNullOrEmpty(roleId))
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

        //    UsersAdmin currUsersAdmin = new UsersAdmin();
        //    List<UsersAdminData> currUsersAdminDataList = currUsersAdmin.GetUsersAdminViewData(userId, roleId);


        //    List<UsersAdminVM> currUsersAdminVMs = currUsersAdminDataList != null && currUsersAdminDataList.Count > 0 ? ObjectMapper.GetMappedUsersAdminVMList(currUsersAdminDataList) : null;

        //    UsersAdminVM currUsersAdminVM = currUsersAdminVMs == null || currUsersAdminVMs.Count == 0 ? new UsersAdminVM() : currUsersAdminVMs[0];

        //    return View(currUsersAdminVM);
        //}

        [System.Web.Http.HttpPost]
        public ActionResult Edit(UsersAdminVM usersAdminVM)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Check if any field edited by user on the font-end.
                if (!IsDirty(usersAdminVM))
                    return RedirectToAction("Index", "User");

                var userDO = uow.UserRepository.GetQuery().FirstOrDefault(u => u.Id == usersAdminVM.UserId);
                userDO.Id = usersAdminVM.UserId;
                userDO.FirstName = usersAdminVM.FirstName;
                userDO.LastName = usersAdminVM.LastName;
                userDO.EmailAddress = new EmailAddressDO()
                {
                    Id = usersAdminVM.EmailAddressID,
                    Address = usersAdminVM.EmailAddress
                };

                userDO.EmailAddressID = usersAdminVM.EmailAddressID;
                userDO.UserName = usersAdminVM.EmailAddress;

                // Set RoleId & UserId if role is changed on the font-end other wise IdentityUserRole is set to null and user's role will not be updated.
                if (usersAdminVM.RoleId != usersAdminVM.PreviousRoleId)
                {
                    uow.AspNetUserRolesRepository.AssignRoleIDToUser(usersAdminVM.RoleId, usersAdminVM.UserId);
                }

                uow.SaveChanges();
                return RedirectToAction("Index", "User");
            }
        }

        private bool IsDirty(UsersAdminVM usersAdminVM)
        {
            bool blnIsDirty = false;

            blnIsDirty = usersAdminVM.FirstName != usersAdminVM.PreviousFirstName ? true : usersAdminVM.LastName != usersAdminVM.PreviousLasttName ? true : usersAdminVM.FirstName != usersAdminVM.PreviousFirstName ? true : usersAdminVM.LastName != usersAdminVM.PreviousLasttName ? true : usersAdminVM.EmailAddress != usersAdminVM.PreviousEmailAddress ? true : usersAdminVM.RoleId != usersAdminVM.PreviousRoleId ? true : false;

            return blnIsDirty;
        }
    }
}