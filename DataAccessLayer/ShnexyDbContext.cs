using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Shnexy.Models;

namespace Shnexy.DataAccessLayer
{


    public class ShnexyDbContext : DbContext
    {

        public ShnexyDbContext() : base("ShnexyDbContext")
        {
            // Database.SetInitializer(new DropCreateDatabaseAlways<ShnexyDbContext>());
        }
        public DbSet<User> Users { get; set; }

        public DbSet<Message> Messages { get; set; }
        public DbSet<Registration> Registrations { get; set; }

        public DbSet<Service> Services { get; set; }
        public DbSet<Profile> Profiles { get; set; }

        public DbSet<Address> Addresses { get; set; }

        public DbSet<Queue> Queues { get; set; }
    }
}