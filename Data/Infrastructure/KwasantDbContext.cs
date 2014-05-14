using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Data.Migrations;

namespace Data.Infrastructure
{


    public class KwasantDbContext : DbContext, IDBContext
    {       
        //Do not change this value! If you want to change the database you connect to, edit your web.config file
        public KwasantDbContext()
            : base("name=KwasantDB")
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<KwasantDbContext, MigrationConfiguration>()); 
        }

        public override int SaveChanges()
        {
            ChangeTracker.DetectChanges();
            //Debug code!
            List<object> adds = ChangeTracker.Entries().Where(e => e.State == EntityState.Added).Select(e => e.Entity).ToList();
            List<object> deletes = ChangeTracker.Entries().Where(e => e.State == EntityState.Deleted).Select(e => e.Entity).ToList();
            var modifies = ChangeTracker.Entries().Where(e => e.State == EntityState.Modified)
                .Select(e =>
                {
                    const string displayChange = "[{0}]: [{1}] -> [{2}]";
                    List<string> changedValues = new List<String>();
                    foreach (string prop in e.OriginalValues.PropertyNames)
                    {
                        object originalValue = e.OriginalValues[prop];
                        object currentValue = e.CurrentValues[prop];
                        if ((originalValue == null && currentValue != null) ||
                            (originalValue != null && !originalValue.Equals(currentValue)))
                        {
                            changedValues.Add(String.Format(displayChange, prop, originalValue,
                                currentValue));
                        }
                    }

                    string actualName = (e.Entity.GetType().FullName.StartsWith("System.Data.Entity.DynamicProxies") &&
                                         e.Entity.GetType().BaseType != null)
                        ? e.Entity.GetType().BaseType.Name
                        : e.Entity.GetType().FullName;
                    return new
                    {
                        EntityName = actualName,
                        ChangedValue = changedValues
                    };
                })
                .Where(e => e.ChangedValue != null && e.ChangedValue.Count > 0)
                .ToList();

            foreach (DbEntityEntry<ISaveHook> entity in ChangeTracker.Entries<ISaveHook>().Where(e => e.State != EntityState.Unchanged))
            {
                entity.Entity.SaveHook(entity);
            }

            return base.SaveChanges();
        }

        public IDbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            return base.Set<TEntity>();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AttachmentDO>().ToTable("Attachments");
            modelBuilder.Entity<AttendeeDO>().ToTable("Attendees");
            modelBuilder.Entity<BookingRequestDO>().ToTable("BookingRequests");
            modelBuilder.Entity<CalendarDO>().ToTable("Calendars");
            modelBuilder.Entity<CommunicationConfigurationDO>().ToTable("CommunicationConfigurations");
            modelBuilder.Entity<CustomerDO>().ToTable("Customers");
            modelBuilder.Entity<EmailAddressDO>().ToTable("EmailAddresses");
            modelBuilder.Entity<EmailDO>().ToTable("Emails");
            modelBuilder.Entity<EventDO>().ToTable("Events");
            modelBuilder.Entity<InstructionDO>().ToTable("Instructions");
            modelBuilder.Entity<PersonDO>().ToTable("People");
            modelBuilder.Entity<StoredFileDO>().ToTable("StoredFiles");
            modelBuilder.Entity<TrackingStatusDO>().ToTable("TrackingStatuses");
            modelBuilder.Entity<UserDO>().ToTable("Users");

            modelBuilder.Entity<EventDO>()
                .HasMany(ev => ev.Emails)
                .WithMany(e => e.Events)
                .Map(
                    mapping => mapping.MapLeftKey("EventID").MapRightKey("EmailID").ToTable("EventEmail")
                );


            modelBuilder.Entity<BookingRequestDO>()
                .HasMany(ev => ev.Instructions)
                .WithMany()
                .Map(
                    mapping => mapping.MapLeftKey("BookingRequestID").MapRightKey("InstructionID").ToTable("BookingRequestInstruction")
                );
            
            
            modelBuilder.Entity<EmailDO>()
                .HasRequired(e => e.From);

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

            modelBuilder.Entity<TrackingStatusDO>()
                .HasKey(ts => new
                {
                    ts.ForeignTableID,
                    ts.ForeignTableName
                });

            base.OnModelCreating(modelBuilder);
        }

        public IDbSet<AttachmentDO> Attachments { get; set; }

        public IDbSet<AttendeeDO> Attendees { get; set; }

        public IDbSet<BookingRequestDO> BookingRequests { get; set; }

        public IDbSet<CalendarDO> Calendars { get; set; }

        public IDbSet<CommunicationConfigurationDO> CommunicationConfigurations { get; set; }

        public IDbSet<CustomerDO> Customers { get; set; }

        public IDbSet<EmailDO> Emails { get; set; }

        public IDbSet<EmailAddressDO> EmailAddresses { get; set; }

        public IDbSet<EventDO> Events { get; set; }

        public IDbSet<InstructionDO> Instructions { get; set; }

        public IDbSet<PersonDO> People { get; set; }

        public IDbSet<StoredFileDO> StoredFiles { get; set; }

        public IDbSet<UserDO> Users { get; set; }

        public IDbSet<TrackingStatusDO> TrackingStatuses { get; set; }
    }
}