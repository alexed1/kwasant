using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.Annotations;
using System.Linq;
using Microsoft.AspNet.Identity.EntityFramework;

using Data.Entities;
using Data.Interfaces;
using Data.Migrations;

namespace Data.Infrastructure
{
    public class KwasantDbContext : IdentityDbContext<IdentityUser>, IDBContext
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

            var saveResult = base.SaveChanges();
            foreach (var entity in ChangeTracker.Entries().Where(e => e.State != EntityState.Unchanged))
            {
                entity.State = EntityState.Unchanged;
            }

            return saveResult;
        }

        public IDbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            return base.Set<TEntity>();
        }

        public IUnitOfWork UnitOfWork { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AttachmentDO>().ToTable("Attachments");
            modelBuilder.Entity<AttendeeDO>().ToTable("Attendees");
            modelBuilder.Entity<BookingRequestDO>().ToTable("BookingRequests");
            modelBuilder.Entity<CalendarDO>().ToTable("Calendars");
            modelBuilder.Entity<CommunicationConfigurationDO>().ToTable("CommunicationConfigurations");
            modelBuilder.Entity<RecipientDO>().ToTable("Recipients");
            modelBuilder.Entity<EmailAddressDO>().ToTable("EmailAddresses");
            modelBuilder.Entity<EmailDO>().ToTable("Emails");
            modelBuilder.Entity<EventDO>().ToTable("Events");
            modelBuilder.Entity<InstructionDO>().ToTable("Instructions");
            modelBuilder.Entity<StoredFileDO>().ToTable("StoredFiles");
            modelBuilder.Entity<TrackingStatusDO>().ToTable("TrackingStatuses");
            modelBuilder.Entity<IdentityUser>().ToTable("IdentityUsers");
            modelBuilder.Entity<UserDO>().ToTable("Users");

            modelBuilder.Entity<EmailDO>()
                .HasRequired(a => a.From)
                .WithMany()
                .HasForeignKey(a => a.FromID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<UserDO>()
                .Property(u => u.EmailAddressID)
                .IsRequired()
                .HasColumnAnnotation(
                    "Index",
                    new IndexAnnotation(new IndexAttribute("IX_User_EmailAddress", 1) {IsUnique = true}));

            modelBuilder.Entity<EmailAddressDO>()
                .Property(ea => ea.Address)
                .IsRequired()
                .HasColumnAnnotation(
                    "Index",
                    new IndexAnnotation(new IndexAttribute("IX_EmailAddress_Address", 1) {IsUnique = true}));

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
            
            modelBuilder.Entity<AttachmentDO>()
                .HasRequired(a => a.Email)
                .WithMany(e => e.Attachments)
                .HasForeignKey(a => a.EmailID);

            modelBuilder.Entity<EventDO>()
                .HasMany(e => e.Attendees)
                .WithRequired(a => a.Event)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<TrackingStatusDO>()
                .HasKey(ts => new
                {
                    ts.Id,
                    ts.ForeignTableName
                });

            base.OnModelCreating(modelBuilder);
        }
    }
}