using System;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using FluentValidation;
using KwasantCore.Managers.APIManager.Packagers;
using KwasantCore.Managers.APIManager.Packagers.DataTable;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using KwasantTest.Fixtures;
using KwasantWeb.Controllers;
using Moq;
using NUnit.Framework;
using StructureMap;
using Utilities;
using System.Web.Mvc;
using System.Net.Mail;
using System.Linq;
using KwasantCore.Managers.APIManager.Packagers.Kwasant;

namespace KwasantTest.Controllers
{
    class BookingRequestControllerTests
    {
        public IUnitOfWork _uow;
        private FixtureData _fixture;
        private DataTablesPackager _datatables;

        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();

            _fixture = new FixtureData();
            _datatables = new DataTablesPackager();
        }

        private void AddTestRequestData()
        {
            MailMessage message = new MailMessage(new MailAddress("customer@gmail.com", "Mister Customer"), new MailAddress("kwa@sant.com", "Bookit Services")) { };

            BookingRequestRepository bookingRequestRepo = _uow.BookingRequestRepository;
            BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);
            (new BookingRequest()).ProcessBookingRequest(_uow, bookingRequest);
        }

        [Test]
        [Category("BRM")]
        public void ShowUnprocessedRequestTest()
        {
            BookingRequestController controller = new BookingRequestController();
            JsonResult jsonResultActual = controller.ShowUnprocessed() as JsonResult;

            string jsonResultExpected = _datatables.Pack((new BookingRequest()).GetUnprocessed(_uow.BookingRequestRepository));
            Assert.AreEqual(jsonResultExpected, jsonResultActual.Data.ToString());

            AddTestRequestData();
            JsonResult jsonResultActualProcessed = controller.ShowUnprocessed() as JsonResult;
            string jsonResultExpectedProcessed = _datatables.Pack((new BookingRequest()).GetUnprocessed(_uow.BookingRequestRepository));
            Assert.AreEqual(jsonResultExpectedProcessed, jsonResultActualProcessed.Data.ToString());

        }

        [Test]
        [Category("BRM")]
        public void SetStatusTest()
        {
            AddTestRequestData();

            BookingRequestController controller = new BookingRequestController();
            int id = _uow.BookingRequestRepository.GetAll().FirstOrDefault().Id;
            JsonResult jsonResultActual = controller.SetStatus(id, "invalid") as JsonResult;
            Assert.AreEqual("Success", ((Error)jsonResultActual.Data).Name);

        }

        [Test]
        [Category("BRM")]
        public void GetBookingRequestsTest()
        {
            AddTestRequestData();

            BookingRequestController controller = new BookingRequestController();
            int id = _uow.BookingRequestRepository.GetAll().FirstOrDefault().Id;
            string jsonResultExpected = _datatables.Pack((new BookingRequest()).GetBookingRequests(_uow.BookingRequestRepository, id));
            JsonResult jsonResultActual = controller.GetBookingRequests(id) as JsonResult;
            Assert.AreEqual(jsonResultExpected, jsonResultActual.Data.ToString());
        }

        
    }
}
