using System.Web.Mvc;
using Data.Validators;
using FluentValidation;
using Data.Entities;
using Data.Infrastructure;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Validations;
using StructureMap;
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

                MeetingInformationDO meetingInformationDO = new MeetingInformationDO(meetingInfo);

                MeetingInformationValidator meetingInformationValidator = new MeetingInformationValidator();
                meetingInformationValidator.ValidateAndThrow(meetingInformationDO); 
               
                return RedirectToAction("Generate", "BookingRequest", new { emailAddress = emailAddress, meetingInfo = meetingInfo });
            }
            catch (System.Exception ex)
            {
                string[] errorMsg = ex.Message.Split('-');
                if (errorMsg.Length > 2)
                    result = errorMsg[2];
                else
                    result = ex.Message;
            }
            return Content(result);
        }
    }
}