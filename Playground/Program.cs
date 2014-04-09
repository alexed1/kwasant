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
using StructureMap;

namespace Playground
{
    class Program
    {
        /// <summary>
        /// This is a sandbox for devs to use. Useful for directly calling some library without needing to launch the main application
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            StructureMapBootStrapper.ConfigureDependencies(String.Empty);
            var con = new ShnexyDbContext();
             con.Database.Initialize(true);


            //var uow = ObjectFactory.GetInstance<IUnitOfWork>();
            //var emailRepo = new EmailRepository(uow);
            //var e = new Email();
            //e.To = new List<EmailAddress>
            //{
            //    new EmailAddress { Address = "Rob", Name = "Rob", ToEmail = e},
            //    new EmailAddress { Address = "Rob2", Name = "Rob2", ToEmail = e}
            //};
            //e.From = new EmailAddress {Address = "FromRob", Name = "FromRob"};
            //e.Status = new EmailStatusConstants {Value = "queued"};
            //emailRepo.Add(e);
            //emailRepo.UnitOfWork.SaveChanges();
            //;

            //var fromEmail = emailRepo.GetQuery().Select(e => e.From).ToList();
            new OutboundEmailHandler().Start();
        }
    }
}
