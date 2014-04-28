using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Daemons;
using Data.Entities;
using Data.Entities.Enumerations;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Services;
using KwasantCore.StructureMap;
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
            StructureMapBootStrapper.ConfigureDependencies("test"); //set to either "test" or "dev"
            
            Database.SetInitializer(new ShnexyInitializer());
            ShnexyDbContext db = new ShnexyDbContext();
            db.Database.Initialize(true);

            //var omd = new OperationsMonitoringDaemon();
            //omd.Start();
            IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>();
            TrackingStatusRepository trackingStatusRepo = new TrackingStatusRepository(uow);
            EmailRepository emailRepo = new EmailRepository(uow);

            TrackingStatus<EmailDO> ts = new TrackingStatus<EmailDO>(trackingStatusRepo, emailRepo);

            var newEmail = new EmailDO() {From = new EmailAddressDO()};
            emailRepo.Add(newEmail);
            ts.SetStatus(newEmail, TrackingStatus.PROCESSED);
            uow.SaveChanges();
            ;
        }
    }
}
