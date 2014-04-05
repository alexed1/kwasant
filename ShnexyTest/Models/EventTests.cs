using System.Data.Entity;
using Data.DataAccessLayer.Infrastructure;
using Data.DataAccessLayer.Interfaces;
using Data.DataAccessLayer.Repositories;
using Data.DataAccessLayer.StructureMap;
using Data.Models;
using NUnit.Framework;
using ShnexyTest.Fixtures;
using StructureMap;
using Event = Data.Models.Event;

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
            Event curEvent = _fixture.TestEvent();
            //load the corresponding test customer
            Customer curCustomer = _fixture.TestCustomer();

            //persist the customer to test the database.
            curCustomer.Add();
            customerRepo.UnitOfWork.SaveChanges();

            //Create new email
            //Call Email#Configure
            //Call Email#Dispatch
            curEvent.Dispatch();

            //Verify success
            //use imap to load unread messages from the test customer account
            //verify that one of the messages is a proper ICS message
            //retry every 15 seconds for 1 minute

            

            //create an Email message addressed to the customer and attach the file.
            
           



            //skip for v.1: add EmailID to outbound queue



        }
    }
}
