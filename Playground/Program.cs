using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Daemons;
using Data.DataAccessLayer.Infrastructure;
using Data.DataAccessLayer.Interfaces;
using Data.DataAccessLayer.Repositories;
using Data.DataAccessLayer.StructureMap;
using Data.Models;
using DBTools;
using StructureMap;

namespace Playground
{
    class Program
    {
        /// <summary>
        /// This is a sandbox for devs to use. Useful for directly calling some library without needing to launch the main application
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            StructureMapBootStrapper.ConfigureDependencies(String.Empty);
            var con = new ShnexyDbContext();
            con.Database.Initialize(true);

            var uow = ObjectFactory.GetInstance<IUnitOfWork>();
            var invRepo = new InvitationRepository(uow);
            var attendeesRepo = new AttendeeRepository(uow);
            var attendees =
                new List<Attendee>
                {
                    new Attendee
                    {
                        EmailAddress = "robert.rudman@serraview.com",
                        Name = "Robert Rudman",
                    },
                    new Attendee
                    {
                        EmailAddress = "joeblow@gmail.com",
                        Name = "Joe Blow",
                    },
                    new Attendee
                    {
                        EmailAddress = "rjrudman@gmail.com",
                        Name = "Robert Rudman GMAIL",
                    }
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
            EmailHelper.DispatchInvitation(uow, invitation);
        }
    }
}
