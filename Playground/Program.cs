using System;
using System.Data.Entity;
using Daemons;
using Data.Infrastructure;
using KwasantCore.StructureMap;
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

            //var a = new TableStorageAppender();

            
            //log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType).Info("Test message!");

            Logger.GetLogger().Error("MY TEST ERROR!!!!!");
            Logger.GetLogger().Info("SOME INFO!!!");


            Console.ReadKey();
        }
    }
}
