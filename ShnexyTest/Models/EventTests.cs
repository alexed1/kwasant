using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Core;
using NUnit.Framework;
using Shnexy;
using Shnexy.DataAccessLayer;
using Shnexy.DataAccessLayer.Interfaces;
using Shnexy.DataAccessLayer.Repositories;
using Shnexy.DataAccessLayer.StructureMap;
using Shnexy.DDay.iCal;
using ShnexyTest.Fixtures;
using Shnexy.Models;
using StructureMap;
using System.Web.Mvc;

using System.Data.Entity;
using Shnexy.DataAccessLayer;
using Shnexy.DataAccessLayer.StructureMap;

namespace ShnexyTest.Models
{


    [TestFixture]
    public class EventTests
    {
        public ICustomerRepository customerRepo;
        public IUnitOfWork _uow;
        private FixtureData _fixture;

        [SetUp]
        public void Setup()
        {

            
            Database.SetInitializer(new DropCreateDatabaseAlways<ShnexyDbContext>());
             
       
            StructureMapBootStrapper.ConfigureDependencies("test");


            _uow = ObjectFactory.GetInstance<IUnitOfWork>();

            _uow.Db.Database.Initialize(true);

            customerRepo = new CustomerRepository(_uow);

            _fixture = new FixtureData(_uow);
        }

        //this is a core integration test: get the ics message through
        [Test]
        [Category("Event")]
        public void Event_Dispatch_CanSendICS()
        {

            //load the test event
            Shnexy.Models.Event curEvent = _fixture.TestEvent();
            //load the corresponding test customer
            Customer curCustomer = _fixture.TestCustomer();



            //persist the customer to test the database.

            curCustomer.Add();
            customerRepo.UnitOfWork.SaveChanges();

            curEvent.Dispatch();
            //use imap to load unread messages from the test customer account
            //verify that one of the messages is a proper ICS message
            //retry every 15 seconds for 1 minute



        }
    }
}
