using System.Web.Mvc;
using Data.Validators;
using FluentValidation;
using Data.Entities;
using System.Collections.Generic;
using KwasantWeb.Filters;
using Data.Infrastructure;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using StructureMap;
using KwasantCore.Services;
using System.Net.Mail;
using Utilities.Logging;

namespace KwasantWeb.Controllers
{

    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return Redirect("/");
            
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }


        
        //  EmailAddress  is valid then send mail .    
        // return "success" or  error 
        public ActionResult ProcessSubmittedEmail(string name, string emailId, string message)
        {
            string result = "";
            try
            {
                EmailAddressDO emailAddressDO = new EmailAddressDO(emailId);

                EmailAddressValidator emailAddressValidator = new EmailAddressValidator();
                emailAddressValidator.ValidateAndThrow(emailAddressDO);

                EmailAddress emailAddress = new EmailAddress();

                using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    Email email = new Email(uow);
                    emailAddress.ConvertFromMailAddress(uow, new MailAddress(emailId, name));
                    EmailDO emailDO = email.GenerateBasicMessage(emailAddressDO, message);
                    email.Send(emailDO);
                }
                result = "success";
            }
            catch (ValidationException ex)
            {
                result = "You need to provide a valid Email Address.";
            }
            catch (System.Exception ex)
            {
                result = "Something went wrong with our effort to send this message. Sorry! Please try emailing your message directly to info@kwasant.com";
                Logger.GetLogger().Error("Error processing a home page email form submission.", ex);
            }
            return Content(result);
        }
    }
}