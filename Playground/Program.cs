using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Repositories;
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
                    EmailAddress = "alex@edelstein.org",
                    EventID = evDO.Id,
                    Event = evDO,
                    Name = "Alex Edelstein",
               },
                               new AttendeeDO
                {
                    EmailAddress = "alexed@yahoo.com",
                    EventID = evDO.EventID,
                    Event = evDO,
                    Name = "Katie Vanderdrift",

               },        
               new AttendeeDO
                {
                    EmailAddress = "kwasanttest@icloud.com",
                    EventID = evDO.EventID,
                    Event = evDO,
                    Name = "KwasantTest",
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
            evDO.Summary = "aaaaa Gmail and outlook works";
            evDO.StartDate = DateTime.Now.AddHours(3);
            evDO.EndDate = DateTime.Now.AddHours(4);

            new Event().Dispatch(evDO);
        }
    }
}
