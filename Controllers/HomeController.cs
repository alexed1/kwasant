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


        /// <summary>
        /// This method is used to call EmailAddressValidator , if emailAddress is valid then call SendMailContact()  Method  in email class.    
        /// </summary>
        /// <returns>Contant Result if Email sent successfully than return success otherwise return Appropriate error </returns>
        public ActionResult ProcessSubmittedEmail(string name, string emailAddress, string subject, string message)
        {
            string result = "";
            try
            {
               var EmailAddressDO = new EmailAddressDO();
               EmailAddressDO.Address = emailAddress;
               EmailAddressValidator emailAddressValidator = new EmailAddressValidator();
               emailAddressValidator.ValidateAndThrow(EmailAddressDO);
               using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
               {
                   Email email = new Email(uow);
                   email.SendMailContact(name, emailAddress, subject, message);
               }
               result ="success";
            }
            catch (System.Exception ex)
            {
                string[] errorMsg =  ex.Message.Split('-');
                if (errorMsg.Length > 2)
                    result = errorMsg[2];
                else
                    result=ex.Message;
            }
            return Content(result);
        }
    }
}