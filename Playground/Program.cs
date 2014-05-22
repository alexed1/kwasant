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

            Expression<Func<EventDO, int>> selector = a => a.Id;
            var name = (selector.Body as dynamic).Member.Name;
        }
    }
}
