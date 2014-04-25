using System.Data.Entity;
using System.Linq;
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

            var uow = ObjectFactory.GetInstance<IUnitOfWork>();
            var trackingStatusRepo = new TrackingStatusRepository(uow);
            var emailRepo = new EmailRepository(uow);

            var ts = new TrackingStatus<EmailDO>(trackingStatusRepo, emailRepo);
            var res = ts.GetForeignEntitiesWithoutStatus();
            var resThree = ts.GetForeignEntitiesWhereTrackingStatus(trackingStatusDO => trackingStatusDO.Value == "ASD");
            var t = 1;
        }
    }
}
