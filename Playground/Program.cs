using System;
using Data.Infrastructure;
using KwasantCore.StructureMap;
using log4net.Config;
using UtilitiesLib.Logging;

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

            Logger.GetLogger().Error("MY TEST ERROR!!!!!");
            Logger.GetLogger().Info("SOME INFO!!!");


            Console.ReadKey();
        }
    }
}
