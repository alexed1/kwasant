using System;
using System.Collections.Generic;
using System.Data.Entity;
using Data.DataAccessLayer.Infrastructure;
using Data.DataAccessLayer.Interfaces;
using Data.DataAccessLayer.Repositories;
using Data.DataAccessLayer.StructureMap;
using Data.Models;
using DBTools;
using NUnit.Framework;
using ShnexyTest.Fixtures;
using StructureMap;
using Event = Data.Models.Event;

namespace ShnexyTest.Models
{


    [TestFixture]
    public class InvitationTests
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
        [Category("Invitation")]
        public void Event_Dispatch_CanSendICS()
        {
            var invRepo = new InvitationRepository(_uow);
            var attendeesRepo = new AttendeeRepository(_uow);
            var attendees =
                new List<Attendee>
                {
                    _fixture.TestAttendee1(),
                    _fixture.TestAttendee2()
                };
            attendees.ForEach(attendeesRepo.Add);

            var invitation = new Invitation
            {
                Description = "This is my test invitation",
                Summary = @"My test invitation",
                Where = @"Some place!",
                StartDate = DateTime.Today.AddMinutes(5),
                EndDate = DateTime.Today.AddMinutes(15),
                Attendees = attendees,
                Emails = new List<Email>()
            };
            invRepo.Add(invitation);
            EmailHelper.DispatchInvitation(_uow, invitation);
            //Verify success
            //use imap to load unread messages from the test customer account
            //verify that one of the messages is a proper ICS message
            //retry every 15 seconds for 1 minute

            

            //create an Email message addressed to the customer and attach the file.
            
           



            //skip for v.1: add EmailID to outbound queue



        }
    }
}
