using System;
using Data.DataAccessLayer.Infrastructure;
using Data.DataAccessLayer.StructureMap;

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
            StructureMapBootStrapper.ConfigureDependencies(String.Empty);
            var con = new ShnexyDbContext();
            con.Database.Initialize(true);
        }
    }
}
