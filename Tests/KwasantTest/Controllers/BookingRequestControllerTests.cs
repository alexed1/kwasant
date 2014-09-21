using System;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Managers.APIManager.Packagers.DataTable;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using KwasantTest.Fixtures;
using KwasantWeb.Controllers;
using NUnit.Framework;
using StructureMap;
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
            MailMessage message = new MailMessage(new MailAddress("customer@gmail.com", "Mister Customer"), new MailAddress("kwa@sant.com", "Bookit Services")) { Body = String.Empty };

            BookingRequestRepository bookingRequestRepo = _uow.BookingRequestRepository;
            BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);
            (new BookingRequest()).Process(_uow, bookingRequest);
        }

        [Test]
        [Category("BRM")]
        public void ShowUnprocessedRequestTest()
        {
            BookingRequestController controller = new BookingRequestController();
            JsonResult jsonResultActual = controller.ShowUnprocessed() as JsonResult;

            string jsonResultExpected = _datatables.Pack((new BookingRequest()).GetUnprocessed(_uow));
            Assert.AreEqual(jsonResultExpected, jsonResultActual.Data.ToString());

            AddTestRequestData();
            JsonResult jsonResultActualProcessed = controller.ShowUnprocessed() as JsonResult;
            string jsonResultExpectedProcessed = _datatables.Pack((new BookingRequest()).GetUnprocessed(_uow));
            Assert.AreEqual(jsonResultExpectedProcessed, jsonResultActualProcessed.Data.ToString());

        }

        [Test]
        [Category("BRM")]
        public void MarkAsProcessedTest()
        {
            AddTestRequestData();

            BookingRequestController controller = new BookingRequestController();
            int id = _uow.BookingRequestRepository.GetAll().FirstOrDefault().Id;
            JsonResult jsonResultActual = controller.MarkAsProcessed(id) as JsonResult;
            Assert.AreEqual("Success", ((KwasantPackagedMessage)jsonResultActual.Data).Name);
        }

        [Test]
        [Category("BRM")]
        public void InvalidateTest()
        {
            AddTestRequestData();

            BookingRequestController controller = new BookingRequestController();
            int id = _uow.BookingRequestRepository.GetAll().FirstOrDefault().Id;
            JsonResult jsonResultActual = controller.Invalidate(id) as JsonResult;
            Assert.AreEqual("Success", ((KwasantPackagedMessage)jsonResultActual.Data).Name);
        }



        [Test]
        [Category("BRM")]
        public void GetBookingRequestsTest()
        {
            AddTestRequestData();

            BookingRequestController controller = new BookingRequestController();
            int id = _uow.BookingRequestRepository.GetAll().FirstOrDefault().Id;
            string jsonResultExpected = (new { draw = 1, recordsTotal = 1, recordsFiltered = 1, data = _datatables.Pack((new BookingRequest()).GetAllByUserId(_uow.BookingRequestRepository, 0, 10, _uow.BookingRequestRepository.GetAll().FirstOrDefault().User.Id)) }).ToString();
            JsonResult jsonResultActual = controller.GetBookingRequests(id, 1, 0, 10) as JsonResult;
            Assert.AreEqual(jsonResultExpected, jsonResultActual.Data.ToString());
        }

        
    }
}
