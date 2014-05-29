using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Threading.Tasks;
using Data.Entities.Enumerations;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Net.Mail;
using System.Configuration;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using Data.Infrastructure;
using Utilities;
using KwasantWeb.ViewModels;
using KwasantCore.Services;
using KwasantCore.Managers.IdentityManager;

namespace KwasantWeb.Controllers
{
    /// <summary>
    /// Email service
    /// </summary>
    public class KwasantEmailService : IIdentityMessageService
    {
        IUnitOfWork _uow;

        public KwasantEmailService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task SendAsync(IdentityMessage message)
        {
            String senderMailAddress = ConfigurationManager.AppSettings["fromEmail"];

            EmailDO emailDO = new EmailDO();
            emailDO.AddEmailRecipient(EmailParticipantType.TO, Email.GenerateEmailAddress(_uow, new MailAddress(message.Destination)));
            emailDO.AddEmailRecipient(EmailParticipantType.FROM, Email.GenerateEmailAddress(_uow, new MailAddress(senderMailAddress)));

            emailDO.Subject = message.Subject;
            emailDO.HTMLText = message.Body;

            Email userEmail = new Email(_uow, emailDO);
            userEmail.Send();
        }
    }

     [Authorize]
    public class AccountController : Controller
    {
        private IUnitOfWork _uow;
        private Account _account;
        private UserManager<UserDO> _userManager;

        public AccountController(IUnitOfWork uow)
        {
            _uow = uow;
            _account = new Account(_uow);
            _userManager = new UserManager<UserDO>(new UserStore<UserDO>(_uow.Db as KwasantDbContext));

            var provider = new Microsoft.Owin.Security.DataProtection.DpapiDataProtectionProvider("Sample");
            _userManager.UserTokenProvider = new Microsoft.AspNet.Identity.Owin.DataProtectorTokenProvider<UserDO>(provider.Create("EmailConfirmation"));
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
            _account.LogOff();
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
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            try
            { 
                if (ModelState.IsValid)
                {
                    var curUserDO = new UserDO()
                    {
                        UserName = model.Email.Trim(),
                        PersonDO = new PersonDO()
                        {
                            EmailAddress = _uow.EmailAddressRepository.GetOrCreateEmailAddress(model.Email.Trim()),
                            FirstName = model.Email.Trim()
                        }
                    };
                    curUserDO.Password = model.Password.Trim();
                    curUserDO.EmailConfirmed = true; //this line essentially disables email confirmation

                    RegistrationStatus curRegStatus = await _account.Register(curUserDO);
                    if (curRegStatus == RegistrationStatus.UserMustLogIn)
                    {
                        ModelState.AddModelError("", "You are already registered with us. Please login.");
                    }
                    else
                    {
                        //await SendEmailConfirmation(curUserDO); email confirmation is currently turned off
                        _account.SendWelcomeMessage(curUserDO);
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            catch(ApplicationException appEx)
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
                    UserDO curUserDO = new UserDO();
                    curUserDO.UserName = model.Email.Trim();
                    curUserDO.Password = model.Password.Trim();

                    LoginStatus curLoginStatus = await _account.Login(curUserDO, model.RememberMe);
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
                            if (curUserDO.EmailConfirmed == false)
                            {
                                ModelState.AddModelError("", "Please accept the confirmation mail sent to you to activate your account.");
                            }
                            else if (curLoginStatus == LoginStatus.Successful)
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
            string code = await _userManager.GenerateEmailConfirmationTokenAsync(curUserDO.Id);
            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = curUserDO.Id, code = code }, protocol: Request.Url.Scheme);
            _userManager.EmailService = new KwasantEmailService(_uow);
            await _userManager.SendEmailAsync(curUserDO.Id, "Confirm your account", "Please confirm your account by clicking this link: <a href=\"" + callbackUrl + "\">Click here</a>");
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ConfirmEmail(string userId, string code)
        {
            string returnViewName = "RegistrationSuccessful";
            try
            {
                UserDO curUserDO = _uow.UserRepository.FindOne(u => u.Id == userId);
                if (curUserDO != null)
                {
                    curUserDO.EmailConfirmed = true;
                    _uow.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                returnViewName = "Register";
            }

            return RedirectToAction(returnViewName);
        }
    }
}