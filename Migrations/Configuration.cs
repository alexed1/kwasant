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

            //create a registration for the 871skype account
            var registration = new Registration(1, "14158710872", "vvAfia/L8DxRsw6l7gRS8L0Tg5U=");
            Profile curProfile = context.Profiles.First(profile => profile.UserId.Equals(2)); //this is clumsy, but I don't want to sink more time trying to figure out complex queries
            curProfile.Registrations.Add(registration);

            //create addresses
            var addresses = new List<Address>();
            
            Address address806 = new Address();
            address806.Body = "14158067915";
            address806.ServiceName = "WhatsApp";
            addresses.Add(address806);

            Address address871 = new Address();
            address871.Body = "14158710872";
            address871.ServiceName = "WhatsApp";
            addresses.Add(address871);
            //careful, the addresses get saved as part of each message, so if you save them here, you get them twice. problematic.        


            List<Message> messages = new List<Message>();

            //create a message aimed at the 4158067915 address
            Message message = new Message();
            message.RecipientList.Add(address806);
            message.Sender = address871;
            message.Body = "Watson, Come here, please! ";
            
            messages.Add(message);
            messages.ForEach(m => context.Messages.AddOrUpdate(u => u.Body, m));
           
           
            context.SaveChanges();

            List<Queue> queues = new List<Queue>();

            Queue queue = new Queue();
            queue.ServiceName = "WhatsApp";
            queue.MessageList.Add(message.Id);
            queues.Add(queue);
            queues.ForEach(m => context.Queues.AddOrUpdate(u => u.ServiceName, m));
            context.SaveChanges();
        }
    }
}
