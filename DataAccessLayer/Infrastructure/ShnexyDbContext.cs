using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Mail;
using System.Web;
using Shnexy.Models;

namespace Shnexy.DataAccessLayer
{


    public class ShnexyDbContext : DbContext
    {

        public ShnexyDbContext(string dbName) : base(dbName)
        {
            // Database.SetInitializer(new DropCreateDatabaseAlways<ShnexyDbContext>());
        }
        public DbSet<User> Users { get; set; }

        public DbSet<Email> Emails { get; set; }
        
        public DbSet<Registration> Registrations { get; set; }

        public DbSet<Service> Services { get; set; }
        public DbSet<Profile> Profiles { get; set; }

        public DbSet<EmailAddress> EmailAddresses { get; set; }

        public System.Data.Entity.DbSet<Shnexy.Models.AppointmentTable> AppointmentTables { get; set; }
        

      
    }

    public class DevContext : ShnexyDbContext
    {
        public DevContext() : base("localShnexyDb")
        {
            
        }

    }
    public class TestContext : ShnexyDbContext
    {
        public TestContext()
            : base("Shnexy_TEST_Db")
        {

        }

    }

}