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

        [Test]
        [Category("BRM")]
        public void ShowUnprocessedRequestTest()
        {
            //BookingRequestController controller = new BookingRequestController();
            //ActionResult jsonResult = controller.ShowUnprocessedRequest();
            
            //string res = _datatables.Pack(BookingRequest.GetUnprocessed(_uow.BookingRequestRepository));
            //Assert.AreEqual(_datatables.Pack(BookingRequest.GetUnprocessed(_uow.BookingRequestRepository)), jsonResult.ToString());
        }
    }
}
