using System;
using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;
using KwasantWeb.Controllers;
using KwasantWeb.ViewModels;
using NUnit.Framework;
using StructureMap;
using Utilities;

namespace KwasantTest.Controllers
{
    public class NegotiationControllerTests : BaseTest
    {
        [Test, ExpectedException(typeof(ArgumentException), ExpectedMessage = @"Invalid email format")]
        public void TestAttendeesRejectedIfInvalidFormat()
        {
            NegotiationController controller = new NegotiationController();
            var negotiationVM = new NegotiationVM();
            negotiationVM.Attendees = new List<String> {"rjrudman@gmail.com,someotheremail@gmail.com"};

            controller.ProcessSubmittedForm(negotiationVM);
        }

        [Test]
        public void TestNewEmailAttendee()
        {
            ObjectFactory.GetInstance<IConfigRepository>().Set("CR_template_for_existing_user", "Mocked Template");

            BookingRequestDO br;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                br = new BookingRequestDO();
                var userDO = new UserDO();
                userDO.EmailAddress = new EmailAddressDO("rjrudman@gmail.com");

                br.From = userDO.EmailAddress;
                br.UserID = userDO.Id;

                uow.BookingRequestRepository.Add(br);
                uow.UserRepository.Add(userDO);
                uow.EmailAddressRepository.Add(userDO.EmailAddress);

                uow.SaveChanges();
            }

            NegotiationController controller = new NegotiationController();
            var negotiationVM = new NegotiationVM();
            negotiationVM.Attendees = new List<String> { "thisismynewemailaddresss@gmail.com" };
            negotiationVM.BookingRequestID = br.Id;

            controller.ProcessSubmittedForm(negotiationVM);
        }

       
    }
}
