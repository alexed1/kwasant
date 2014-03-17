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

        public ShnexyDbContext() : base("localShnexyDb")
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
}