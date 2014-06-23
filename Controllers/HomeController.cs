using System.Web.Mvc;
using Data.Validators;
using FluentValidation;
using Data.Entities;
using Data.Infrastructure;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using StructureMap;
using System;
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
    }
}