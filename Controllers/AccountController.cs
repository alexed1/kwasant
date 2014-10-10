using System;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using KwasantCore.Managers;
using KwasantCore.Services;
using KwasantWeb.ViewModels;
using Microsoft.AspNet.Identity;
using StructureMap;
using Utilities;

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
                        ModelState.AddModelError("", @"You are already registered with us. Please login.");
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
                            ModelState.AddModelError("", @"Invalid Email id or Password.");
                            break;

                        case LoginStatus.ImplicitUser:
                            ModelState.AddModelError("", @"We already have a record of that email address, but No password exists for this Email id. 
Please register first.");
                            break;

                        case LoginStatus.UnregisteredUser:
                            ModelState.AddModelError("", @"We do not have a registered account associated with this email address. 
Please register first.");
                            break;

                        default:
                            if (curLoginStatus == LoginStatus.Successful)
                            {
                                if (!String.IsNullOrEmpty(returnUrl))
                                    return Redirect(returnUrl);

                                bool isAdmin;
                                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                                {
                                    var user = uow.UserRepository.GetQuery().FirstOrDefault(u => u.UserName == username);
                                    isAdmin = uow.AspNetUserRolesRepository.UserHasRole("Admin", user.Id);
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
        
        [System.Web.Http.HttpPost]
        public ActionResult Edit(UserVM usersAdminVM)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var userDO = uow.UserRepository.GetByKey(usersAdminVM.Id);
                userDO.Id = usersAdminVM.Id;
                userDO.FirstName = usersAdminVM.FirstName;
                userDO.LastName = usersAdminVM.LastName;
                userDO.EmailAddress = new EmailAddressDO
                {
                    Id = usersAdminVM.EmailAddressID,
                    Address = usersAdminVM.EmailAddress
                };

                userDO.EmailAddressID = usersAdminVM.EmailAddressID;
                userDO.UserName = usersAdminVM.EmailAddress;

                // Set RoleId & UserId if role is changed on the font-end other wise IdentityUserRole is set to null and user's role will not be updated.
                uow.AspNetUserRolesRepository.AssignRoleIDToUser(usersAdminVM.RoleId, usersAdminVM.Id);

                uow.SaveChanges();
                return RedirectToAction("Index", "User");
            }
        }
    }
}