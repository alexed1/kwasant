using System;
using System.Collections.Generic;
using Data.Entities;
using KwasantCore.StructureMap;
using KwasantWeb.Controllers;
using KwasantWeb.ViewModels;
using NUnit.Framework;

namespace KwasantTest.Controllers
{
    class NegotiationControllerTests
    {

        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
        }

        [Test, ExpectedException(typeof(ArgumentException), ExpectedMessage = @"Invalid email format")]
        public void TestAttendeesRejectedIfInvalidFormat()
        {
            NegotiationController controller = new NegotiationController();
            var negotiationVM = new NegotiationVM();
            negotiationVM.Attendees = new List<String> {"rjrudman@gmail.com,someotheremail@gmail.com"};

            controller.ProcessSubmittedForm(negotiationVM);
        }
    }
}
