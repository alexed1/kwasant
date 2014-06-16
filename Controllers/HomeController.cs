using System.Web.Mvc;
using Data.Validators;
using FluentValidation;
using Data.Entities;
using System.Collections.Generic;

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
        // return success or  error 
        public ActionResult ProcessSubmittedEmail(string name, string emailId, string message)
        {
            string result = "";
            try
            {
                var EmailAddressDO = new EmailAddressDO(emailId);
           
               EmailAddressValidator emailAddressValidator = new EmailAddressValidator();
               emailAddressValidator.ValidateAndThrow(EmailAddressDO);

               EmailAddress emailAddress = new EmailAddress();

               using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
               {
                   Email email = new Email(uow);
                   emailAddress.ConvertFromMailAddress(uow, new MailAddress(emailId, name));
                   EmailDO emaildo = email.GenerateBasicMessage(EmailAddressDO, message);
                   (new Email(uow, emaildo)).Send();
               }
               result ="success";
            }
            catch (System.Exception ex)
            {
                string[] errorMsg = ex.Message.Split('-');
                if (errorMsg.Length > 2)
                    result = errorMsg[2];
                else
                    result=ex.Message;
                Logger.GetLogger().Error("Error processing a home page email form submission.", ex);
            }
            return Content(result);
        }
    }
}