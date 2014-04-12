using System.Data.Entity;
using System.Data.Entity.Infrastructure;
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
            foreach (DbEntityEntry<ISaveHook> entity in ChangeTracker.Entries<ISaveHook>())
            {
                entity.Entity.SaveHook();
            }

            return base.SaveChanges();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AttachmentDO>().ToTable("Attachments");
            modelBuilder.Entity<AttendeeDO>().ToTable("Attendees");
            modelBuilder.Entity<BookingRequestDO>().ToTable("BookingRequests");
            modelBuilder.Entity<CustomerDO>().ToTable("Customers");
            modelBuilder.Entity<EmailAddressDO>().ToTable("EmailAddresses");
            modelBuilder.Entity<EmailDO>().ToTable("Emails");
            modelBuilder.Entity<EmailStatusDO>().ToTable("EmailStatuses");
            modelBuilder.Entity<EventDO>().ToTable("Events");
            modelBuilder.Entity<StoredFileDO>().ToTable("StoredFiles");
            modelBuilder.Entity<UserDO>().ToTable("Users");
            
            modelBuilder.Entity<EmailDO>()
                .HasRequired(e => e.From);
            modelBuilder.Entity<EmailDO>()
                .HasRequired(e => e.StatusDO)
                .WithMany(es => es.Emails)
                .HasForeignKey(e => e.StatusID);

            modelBuilder.Entity<AttachmentDO>()
                .HasRequired(a => a.EmailDO)
                .WithMany(e => e.Attachments)
                .HasForeignKey(a => a.EmailID);

            modelBuilder.Entity<EmailAddressDO>()
                .HasOptional(ea => ea.FromEmailDO)
                .WithRequired(e => e.From)
                .Map(x => x.MapKey("FromEmailAddressID"));

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<AttachmentDO> Attachments { get; set; }

        public DbSet<AttendeeDO> Attendees { get; set; }

        public DbSet<BookingRequestDO> BookingRequests { get; set; }

        public DbSet<CustomerDO> Customers { get; set; }

        public DbSet<EmailDO> Emails { get; set; }

        public DbSet<EmailAddressDO> EmailAddresses { get; set; }

        public DbSet<EmailStatusDO> EmailStatuses { get; set; }
        
        public DbSet<EventDO> Invitations { get; set; }

        public DbSet<StoredFileDO> StoredFiles { get; set; }

        public DbSet<UserDO> Users { get; set; }
        
    }
}