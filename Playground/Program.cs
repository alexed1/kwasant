using System;
using System.Collections.Generic;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using StructureMap;

namespace Playground
{
    public class Program
    {
        /// <summary>
        /// This is a sandbox for devs to use. Useful for directly calling some library without needing to launch the main application
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.LIVE); //set to either "test" or "dev"

            KwasantDbContext db = new KwasantDbContext();
            db.Database.Initialize(true);

            IUnitOfWork unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>();
            var evDO = new EventDO()
            {
            };
            evDO.Attendees = new List<AttendeeDO>
            {
                new AttendeeDO
                {
                    EmailAddress = "rjrudman@gmail.com",
                    EventID = evDO.Id,
                    Event = evDO,
                    Name = "Robert Rudman",
               },
               //new AttendeeDO
               // {
               //     EmailAddress = "alex@edelstein.org",
               //     EventID = evDO.EventID,
               //     Event = evDO,
               //     Name = "Alex Edelstein",
               //},
            };
            evDO.Location = "Skype";
            evDO.Description = "Discuss event visualizations";
            evDO.Summary = "Gmail and outlook works, but...";
            evDO.StartDate = DateTime.Now.AddHours(3);
            evDO.EndDate = DateTime.Now.AddHours(4);

            new Event().Dispatch(evDO);
        }
    }
}
