﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.SqlServer;
using System.Linq;
using Microsoft.AspNet.Identity.EntityFramework;
using Data.Entities;
using Data.Interfaces;
using Data.Migrations;

namespace Data.Infrastructure
{
    public class KwasantDbContext : IdentityDbContext<IdentityUser>, IDBContext
    {
        //This is to ensure compile will break if the reference to sql server is removed
        private static Type m_SqlProvider = typeof(SqlProviderServices);

        public class PropertyChangeInformation
        {
            public String PropertyName;
            public Object OriginalValue;
            public Object NewValue;

            public override string ToString()
            {
                
                const string displayChange = "[{0}]: [{1}] -> [{2}]";
                return String.Format(displayChange, PropertyName, OriginalValue, NewValue);
            }
        }

        public class EntityChangeInformation
        {
            public String EntityName;
            public List<PropertyChangeInformation> Changes;
        }

        //Do not change this value! If you want to change the database you connect to, edit your web.config file
        public KwasantDbContext()
            : base("name=KwasantDB")
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<KwasantDbContext, MigrationConfiguration>()); 
        }


        public List<PropertyChangeInformation> GetEntityModifications<T>(T entity)
            where T : class
        {
            return GetEntityModifications(Entry(entity));
        }

        private List<PropertyChangeInformation> GetEntityModifications<T>(DbEntityEntry<T> entity)
            where T : class
        {
            return GetEntityModifications((DbEntityEntry) entity);
        }

/*
        private List<PropertyChangeInformation> GetEntityModifications(DbEntityEntry entity)
        {
            List<PropertyChangeInformation> changedValues = new List<PropertyChangeInformation>();
            foreach (string prop in entity.OriginalValues.PropertyNames)
            {
                object originalValue = entity.OriginalValues[prop];
                object currentValue = entity.CurrentValues[prop];
                if ((originalValue == null && currentValue != null) ||
                    (originalValue != null && !originalValue.Equals(currentValue)))
                {
                    changedValues.Add(new PropertyChangeInformation {PropertyName = prop, OriginalValue = originalValue, NewValue = currentValue});
                }
            }

            return changedValues;
        }
*/

        public void DetectChanges()
        {
            ChangeTracker.DetectChanges();
        }

        public object[] AddedEntities
        {
            get { return ChangeTracker.Entries().Where(e => e.State == EntityState.Added).Select(e => e.Entity).ToArray(); }
        }

        public object[] ModifiedEntities
        {
            get { return ChangeTracker.Entries().Where(e => e.State == EntityState.Modified).Select(e => e.Entity).ToArray(); }
        }

        public object[] DeletedEntities
        {
            get { return ChangeTracker.Entries().Where(e => e.State == EntityState.Deleted).Select(e => e.Entity).ToArray(); }
        }

/*
        public List<EntityChangeInformation> GetModifiedEntities()
        {
            var res = ChangeTracker.Entries().Where(e => e.State == EntityState.Modified)
                .Select(e =>
                {
                    string actualName = (e.Entity.GetType().FullName.StartsWith("System.Data.Entity.DynamicProxies") &&
                                        e.Entity.GetType().BaseType != null)
                    ? e.Entity.GetType().BaseType.Name
                    : e.Entity.GetType().FullName;

                    return new EntityChangeInformation
                    {
                        EntityName = actualName,
                        Changes = GetEntityModifications(e)
                    };
                })
                .Where(e => e.Changes.Any())
                .ToList();

            return res;
        }
*/

        public override int SaveChanges()
        {
            ChangeTracker.DetectChanges();
            //Debug code!
            List<object> adds = ChangeTracker.Entries().Where(e => e.State == EntityState.Added).Select(e => e.Entity).ToList();
            List<object> deletes = ChangeTracker.Entries().Where(e => e.State == EntityState.Deleted).Select(e => e.Entity).ToList();
            List<object> others = ChangeTracker.Entries().Where(e => e.State != EntityState.Deleted && e.State != EntityState.Added).Select(e => e.Entity).ToList();

            foreach (DbEntityEntry<ISaveHook> entity in ChangeTracker.Entries<ISaveHook>().Where(e => e.State != EntityState.Unchanged))
            {
                entity.Entity.BeforeSave();
            }

            foreach (DbEntityEntry<BookingRequestDO> newBookingRequest in ChangeTracker.Entries<BookingRequestDO>())
            {
                AlertManager.BookingRequestCreated(this.UnitOfWork, newBookingRequest.Entity);
            }

            // do not send to Admins
            foreach (DbEntityEntry<UserDO> newUser in ChangeTracker.Entries<UserDO>()
                .Where(u => u.State.HasFlag(EntityState.Added)))
            {
                if (newUser.Entity.Roles.All(r => UnitOfWork.AspNetRolesRepository.GetByKey(r.RoleId).Name != "Admin"))
                {
                    AlertManager.CustomerCreated(this.UnitOfWork, newUser.Entity);
                }
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
            modelBuilder.Entity<ClarificationRequestDO>().ToTable("ClarificationRequests");            
            modelBuilder.Entity<QuestionDO>().ToTable("Questions");
            modelBuilder.Entity<CommunicationConfigurationDO>().ToTable("CommunicationConfigurations");
            modelBuilder.Entity<RecipientDO>().ToTable("Recipients");
            modelBuilder.Entity<EmailAddressDO>().ToTable("EmailAddresses");
            modelBuilder.Entity<EmailDO>().ToTable("Emails");
            modelBuilder.Entity<EnvelopeDO>().ToTable("Envelopes");
            modelBuilder.Entity<EventDO>().ToTable("Events");
            modelBuilder.Entity<InstructionDO>().ToTable("Instructions");
            modelBuilder.Entity<StoredFileDO>().ToTable("StoredFiles");
            modelBuilder.Entity<TrackingStatusDO>().ToTable("TrackingStatuses");
            modelBuilder.Entity<IdentityUser>().ToTable("IdentityUsers");
            modelBuilder.Entity<UserAgentInfoDO>().ToTable("UserAgentInfos");
            modelBuilder.Entity<UserDO>().ToTable("Users");
            modelBuilder.Entity<FactDO>().ToTable("Facts");
            modelBuilder.Entity<IncidentDO>().ToTable("Incidents");
            modelBuilder.Entity<ConceptDO>().ToTable("Concepts");
            modelBuilder.Entity<NegotiationDO>().ToTable("Negotiations");
            modelBuilder.Entity<AnswerDO>().ToTable("Answers");
            modelBuilder.Entity<RemoteCalendarProviderDO>().ToTable("RemoteCalendarProviders");
            modelBuilder.Entity<RemoteCalendarAuthDataDO>().ToTable("RemoteCalendarAuthData");
            modelBuilder.Entity<RemoteCalendarLinkDO>().ToTable("RemoteCalendarLinks");
            modelBuilder.Entity<QuestionResponseDO>().ToTable("QuestionResponses");

            modelBuilder.Entity<EmailDO>()
                .HasRequired(a => a.From)
                .WithMany()
                .HasForeignKey(a => a.FromID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<EnvelopeDO>()
                .HasRequired(e => e.Email)
                .WithMany()
                .HasForeignKey(e => e.EmailID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<UserDO>()
                .Property(u => u.EmailAddressID)
                .IsRequired()
                .HasColumnAnnotation(
                    "Index",
                    new IndexAnnotation(new IndexAttribute("IX_User_EmailAddress", 1) { IsUnique = true }));

            modelBuilder.Entity<EmailAddressDO>()
                .Property(ea => ea.Address)
                .IsRequired()
                .HasColumnAnnotation(
                    "Index",
                    new IndexAnnotation(new IndexAttribute("IX_EmailAddress_Address", 1) { IsUnique = true }));

            modelBuilder.Entity<EventDO>()
                .HasMany(ev => ev.Emails)
                .WithMany(e => e.Events)
                .Map(
                    mapping => mapping.MapLeftKey("EventID").MapRightKey("EmailID").ToTable("EventEmail")
                );

            modelBuilder.Entity<CalendarDO>()
                .HasMany(ev => ev.BookingRequests)
                .WithMany(e => e.Calendars)
                .Map(
                    mapping => mapping.MapLeftKey("CalendarID").MapRightKey("BookingRequestID").ToTable("BookingRequestCalendar")
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

            modelBuilder.Entity<NegotiationDO>()
                .HasMany(e => e.Questions)
                .WithRequired(a => a.Negotiation);
            
            modelBuilder.Entity<TrackingStatusDO>()
                .HasKey(ts => new
                {
                    ts.Id,
                    ts.ForeignTableName
                });


            modelBuilder.Entity<QuestionDO>()
                .HasMany(e => e.Answers)
                .WithRequired(a => a.Question)
                .WillCascadeOnDelete(true);

            base.OnModelCreating(modelBuilder);
        }
    }
}