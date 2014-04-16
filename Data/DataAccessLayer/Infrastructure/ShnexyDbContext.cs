using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
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
            var entries = ChangeTracker.Entries().ToList();//For debugging
            foreach (DbEntityEntry<ISaveHook> entity in ChangeTracker.Entries<ISaveHook>().Where(e => e.State != EntityState.Unchanged))
            {
                entity.Entity.SaveHook(entity);
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

            modelBuilder.Entity<EventDO>()
                .HasMany(ev => ev.Emails)
                .WithMany(e => e.Events)
                .Map(
                    mapping => mapping.MapLeftKey("EventID").MapRightKey("EmailID").ToTable("EventEmail")
                );
            
            modelBuilder.Entity<EmailDO>()
                .HasRequired(e => e.From);
            modelBuilder.Entity<EmailDO>()
                .HasRequired(e => e.Status)
                .WithMany(es => es.Emails)
                .HasForeignKey(e => e.StatusID);

            modelBuilder.Entity<EmailDO>()
                .HasMany(e => e.To)
                .WithOptional(ea => ea.ToEmail)
                .Map(m => m.MapKey("ToEmailID"));
            modelBuilder.Entity<EmailDO>()
                .HasMany(e => e.BCC)
                .WithOptional(ea => ea.BCCEmail)
                .Map(m => m.MapKey("BCCmailID"));
            modelBuilder.Entity<EmailDO>()
                .HasMany(e => e.CC)
                .WithOptional(ea => ea.CCEmail)
                .Map(m => m.MapKey("CCEmailID"));

            modelBuilder.Entity<AttachmentDO>()
                .HasRequired(a => a.Email)
                .WithMany(e => e.Attachments)
                .HasForeignKey(a => a.EmailID);

            modelBuilder.Entity<EmailAddressDO>()
                .HasOptional(ea => ea.FromEmail)
                .WithRequired(e => e.From)
                .Map(x => x.MapKey("FromEmailAddressID"));

            modelBuilder.Entity<EventDO>()
                .HasMany(e => e.Attendees)
                .WithRequired(a => a.Event)
                .WillCascadeOnDelete(true);

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