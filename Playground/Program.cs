using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Data.Authentication;
using Data.Entities;
using Data.Entities.CTE;
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

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var result = uow.ProfileNodeRepository.GetQuery().Where(pn => pn.Id == 2).WithFullHeirarchy(uow).ToList();
            }
        }

    }
}
