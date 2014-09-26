using System.Linq;
using Data.Authentication;
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


            var at = new AuthorizationToken();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var firstUser = uow.UserRepository.GetQuery().First();
                var tokenLink = at.GetAuthorizationTokenURL(uow, "www.google.com", firstUser);

            }
        }

    }
}
