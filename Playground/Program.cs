using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Daemons;
using Data.Entities;
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

            var omd = new OperationsMonitoringDaemon();
            omd.Start();
            //IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>();
            //TrackingStatusRepository trackingStatusRepo = new TrackingStatusRepository(uow);
            //EmailRepository emailRepo = new EmailRepository(uow);

            //TrackingStatus<EmailDO> ts = new TrackingStatus<EmailDO>(trackingStatusRepo, emailRepo);

            ////ts.SetStatus(null, "Hello!");

            ////List<EmailDO> res = ts.GetEntitiesWithoutStatus().ToList();
            ////IQueryable<EmailDO> resTwo = ts.GetEntitiesWhereTrackingStatus(trackingStatusDO => trackingStatusDO.Value == "ASD");
            ////IQueryable<EmailDO> resFive = ts.GetEntitiesWhereTrackingStatus(trackingStatusDO => trackingStatusDO.Value == "ASD").Where(emailDO => emailDO.Text == "Hello");
            ////IQueryable<EmailDO> resThree = ts.GetEntitiesWithStatus().Where(emailDO => emailDO.Text == "Hello");
            ////IQueryable<EmailDO> resFour = ts.GetEntitiesWithStatus();

            //var keys = (emailRepo.UnitOfWork.Db as IObjectContextAdapter).ObjectContext.CreateObjectSet<EmailDO>().EntitySet.ElementType.FullName;
            //int t = 1;
        }
    }
}
