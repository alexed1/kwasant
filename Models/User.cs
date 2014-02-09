using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using Shnexy.Utilities;

namespace Shnexy.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        
      

        private ShnexyDbContext db = new ShnexyDbContext();



        public User Get(int userId)
        {
            return db.Users.Find(userId);

        }

    }


    public class ShnexyDbContext : DbContext
    {

        static ShnexyDbContext()
        {
            //Database.SetInitializer(new DropCreateDatabaseIfModelChanges<ShnexyDbContext>());
        }
        public DbSet<User> Users { get; set; }

        public DbSet<Message> Messages { get; set; }
        public DbSet<Registration> Registrations { get; set; }

        public DbSet<Service> Services { get; set; }
        public DbSet<Profile> Profiles { get; set; }
    }
}