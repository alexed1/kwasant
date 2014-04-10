using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Data.Models;

namespace Data.DataAccessLayer.Infrastructure
{
    public class ShnexyInitializer : System.Data.Entity.DropCreateDatabaseIfModelChanges<ShnexyDbContext>
    {

        protected override void Seed(ShnexyDbContext context)
        {   
            Debug.WriteLine("in seed");
            //FixtureDataEmail.AddEmailMessage();
        }
    }
}