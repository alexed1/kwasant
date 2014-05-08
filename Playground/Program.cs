using System.Data.Entity;
using Data.Infrastructure;
using KwasantCore.StructureMap;

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
            StructureMapBootStrapper.ConfigureDependencies("dev"); //set to either "test" or "dev"
            
            KwasantDbContext db = new KwasantDbContext();
            db.Database.Initialize(true);
        }
    }
}
