using Shnexy.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Shnexy.Models;

namespace Shnexy.DataAccessLayer
{
    public class ShnexyInitializer : System.Data.Entity.DropCreateDatabaseIfModelChanges<ShnexyDbContext>
    {

        protected override void Seed(ShnexyDbContext context)
        {
        }
    }
}