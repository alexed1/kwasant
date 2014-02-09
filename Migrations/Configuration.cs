namespace shnexy.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Shnexy.Models;
    using System.Collections.Generic;

    internal sealed class Configuration : DbMigrationsConfiguration<Shnexy.Models.ShnexyDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "Shnexy.Models.ShnexyDbContext";
        }

        protected override void Seed(Shnexy.Models.ShnexyDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

            context.Services.AddOrUpdate(
                p => p.Name,
                new Service { Name = "WhatsApp" }
                );

            var users = new List<User>
            {
                new User { Name = "Katie" },
                new User { Name = "Alex" }

            };

            users.ForEach(u => context.Users.AddOrUpdate(p => p.Name, u));
            context.SaveChanges();

            //create an empty Profile for each user
            var profiles = new List<Profile>();
            
            foreach (var user in users)
                {
                    profiles.Add(new Profile { UserId = user.Id });
                }
            

            profiles.ForEach(p => context.Profiles.AddOrUpdate(u => u.UserId, p));
            context.SaveChanges();




        }
    }
}
