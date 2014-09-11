using System.Web.Mvc;
using Data.Validations;
using FluentValidation;
using Data.Entities;
using Data.Interfaces;
using StructureMap;
using KwasantCore.Services;
using System.Net.Mail;
using Utilities.Logging;
using System;
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


        //Validate emailAddress and meetingInfo then call Generate() parameterized method in BookingRequest controller
        public ActionResult ProcessHomePageBookingRequest(string emailAddress, string meetingInfo)
        {
            string result = "";
            try
            {
                var emailAddressDO = new EmailAddressDO(emailAddress);

                EmailAddressValidator emailAddressValidator = new EmailAddressValidator();
                emailAddressValidator.ValidateAndThrow(emailAddressDO);

                if (meetingInfo.Trim().Length < 30)
                    return Content("Meeting information must have at least 30 characters");
               
                return RedirectToAction("Generate", "BookingRequest", new { emailAddress = emailAddress, meetingInfo = meetingInfo });
            }
            catch (ValidationException ex)
            {
                result = "You need to provide a valid Email Address.";
            }
            catch (Exception ex)
            {
                result = "Something went wrong. Sorry about that";
                Logger.GetLogger().Error("Error processing a home page try it out form schedule me", ex);
            }
            return Content(result);
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
                    uow.EnvelopeRepository.ConfigurePlainEmail(emailDO);
                    uow.SaveChanges();
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