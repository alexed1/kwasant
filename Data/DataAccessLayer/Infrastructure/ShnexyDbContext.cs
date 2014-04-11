using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using Data.DataAccessLayer.Interfaces;
using Data.Models;

namespace Data.DataAccessLayer.Infrastructure
{


    public class ShnexyDbContext : DbContext
    {       
        //see web.config for connection string names.
        //azure is AzureDbContext
        public ShnexyDbContext()
            : base("name=ShnexyTESTLocalDb")
        {
            
        }

        public override int SaveChanges()
        {
            ChangeTracker.DetectChanges();
            foreach (var entity in ChangeTracker.Entries<ISaveHook>())
            {
                entity.Entity.SaveHook();
            }

            return base.SaveChanges();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BookingRequest>().ToTable("BookingRequests");
            modelBuilder.Entity<Attachment>().ToTable("Attachments");

            modelBuilder.Entity<Email>()
                .HasRequired(e => e.From);

            modelBuilder.Entity<EmailAddress>()
                .HasOptional(ea => ea.FromEmail)
                .WithRequired(e => e.From)
                .Map(x => x.MapKey("FromEmailAddressID"));

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Invitation> Invitations { get; set; }

        public DbSet<Attendee> Attendees { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<BookingRequest> BookingRequests { get; set; }

        public DbSet<EmailStatus> EmailStatuses { get; set; }

        public DbSet<Email> Emails { get; set; }

        public DbSet<EmailAddress> EmailAddresses { get; set; }

        public DbSet<StoredFile> StoredFiles { get; set; }
    }
}