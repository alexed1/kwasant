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

        private ShnexyDBContext db = new ShnexyDBContext();



        public User Get(int userId)
        {
            return db.Users.Find(userId);

        }

    }


    public class ShnexyDBContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public DbSet<Message> Messages { get; set; }
        public DbSet<Registration> Registrations { get; set; }

        public DbSet<Service> Services { get; set; }
        public DbSet<Profile> Profiles { get; set; }
    }
}