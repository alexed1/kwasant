using System;
using System.Linq;
using System.Linq.Expressions;
using Data.Entities;
using Data.Entities.Enumerations;
using Data.Interfaces;
using Data.Repositories;

namespace Data.Infrastructure
{
    /* Usage example
     
     * IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>();
     * TrackingStatusRepository trackingStatusRepo = new TrackingStatusRepository(uow);
     * EmailRepository emailRepo = new EmailRepository(uow);
     * 
     * EmailDO emailDO = emailRepo.GetQuery().First();
     * 
     * TrackingStatus<EmailDO> ts = new TrackingStatus<EmailDO>(trackingStatusRepo, emailRepo);
     * 
     * ts.GetEntitiesWithoutStatus().ToList();
     * ts.GetEntitiesWhereTrackingStatus(trackingStatusDO => trackingStatusDO.Value == "ASD");
     * ts.GetEntitiesWithStatus().Where(emailDO => emailDO.Text == "Hello");
     * ts.GetEntitiesWithStatus();
     * 
     * ts.GetStatus(emailDO); -- Returns null
     * ts.SetStatus(emailDO, "Hello!");
     * ts.SetStatus(emailDO, "Bye!");
     * ts.GetStatus(emailDO); -- Returns a status row with value 'Bye!'
     * ts.DeleteStatus(emailDO);
     */
    /// <summary>
    /// This class is used to manage TrackingStatuses linked to Entities.
    /// It's a generic implementation, and as such, can be used with any entity in the database, so long as it has a single primary key. Composite keys are not supported.
    /// </summary>
    /// <typeparam name="TForeignEntity">The type of the linked entity (<see cref="EmailDO"></see>, for example)</typeparam>
    public class TrackingStatus<TForeignEntity> : GenericCustomField<TrackingStatusDO, TForeignEntity> 
        where TForeignEntity : class
    {
        public TrackingStatus(TrackingStatusRepository trackingStatusRepo, IGenericRepository<TForeignEntity> foreignRepo) 
            : base(trackingStatusRepo, foreignRepo)
        {
        }

        /// <summary>
        /// Get all entities without a status
        /// </summary>
        /// <returns>IQueryable of entities without any status</returns>
        public IQueryable<TForeignEntity> GetEntitiesWithoutStatus()
        {
            return GetEntitiesWithoutCustomFields();
        }

        public IQueryable<TForeignEntity> GetUnprocessedEntities()
        {
            //Get entities without a status, or with a status marked 'Unprocessed'
            return GetJoinResult(null, null, jr => jr.CustomFieldDO == null || jr.CustomFieldDO.Status == TrackingStatus.UNPROCESSED).Select(jr => jr.ForeignDO);
        }

        /// <summary>
        /// Get all entities with a status confined to the provided predicate
        /// </summary>
        /// <returns>IQueryable of entities with a status confined to the provided predicate</returns>
        public IQueryable<TForeignEntity> GetEntitiesWhereTrackingStatus(Expression<Func<TrackingStatusDO, bool>> customFieldPredicate)
        {
            return GetEntitiesWhereCustomField(customFieldPredicate);
        }

        /// <summary>
        /// Get all entities with a status
        /// </summary>
        /// <returns>IQueryable of entities with a status</returns>
        public IQueryable<TForeignEntity> GetEntitiesWithStatus()
        {
            return GetEntitiesWithCustomField();
        }

        /// <summary>
        /// Sets the status of an entity. If an existing status exists for the entity, the status will be updated. If not, a status will be created.
        /// </summary>
        /// <param name="entityDO">Entity to set the status on</param>
        /// <param name="status">Value of the status</param>
        public void SetStatus(TForeignEntity entityDO, TrackingStatus status)
        {
            GetOrCreateCustomField(entityDO).Status = status;
        }

        /// <summary>
        /// Gets the current status of an entity. If no status exists, null will be returned.
        /// </summary>
        /// <param name="entityDO">The status of the provided entity</param>
        public TrackingStatusDO GetStatus(TForeignEntity entityDO)
        {
            return GetCustomField(entityDO);
        }

        /// <summary>
        /// Deletes the status of an entity. If no status exists, no action will be performed.
        /// </summary>
        /// <param name="entityDO">Entity to delete the status on</param>
        public void DeleteStatus(TForeignEntity entityDO)
        {
            DeleteCustomField(entityDO);
        }
    }
}
