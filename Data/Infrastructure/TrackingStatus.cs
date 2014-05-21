using System;
using System.Linq;
using System.Linq.Expressions;
using Data.Entities;
using Data.Entities.Enumerations;
using Data.Interfaces;

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

    //additional documentation
    //We have our main custom field code in GenericCustomField<TCustomFieldType, TForeignEntity>.
    //    We use TrackingStatus to wrap this code to give us nicer methods ('GetEntitiesWithoutStatus' vs the generic 'GetEntitiesWithoutCustomFields'). T
    //    his means we can also inject predicates into the query. I've re-done the TrackingStatus class to take in a TrackingType as a parameter of each of its methods. 
    //    (I think we should use an Enum rather than a string; but either way, the idea is the same).
    //Now we do something like this:
    //TrackingStatus<BookingRequestDO> ts = new TrackingStatus<BookingRequestDO>(trackingStatusRepo, bookingRequestRepo);
    //List<BookingRequestDO> unprocessedBookingRequests = ts.GetUnprocessedEntities(TrackingType.BOOKING_STATE).ToList();
    //for each(var br in unprocessedBookingRequests)
    //ts.SetStatus(TrackingType.BOOKING_STATE, br, TrackingStatus.PROCESSED)

    /// <summary>
    /// This class is used to manage TrackingStatuses linked to Entities.
    /// It's a generic implementation, and as such, can be used with any entity in the database, so long as it has a single primary key. Composite keys are not supported.
    /// </summary>
    /// <typeparam name="TForeignEntity">The type of the linked entity (<see cref="EmailDO"></see>, for example)</typeparam>
    public class TrackingStatus<TForeignEntity> : GenericCustomField<TrackingStatusDO, TForeignEntity> 
        where TForeignEntity : class
    {
        public TrackingStatus(IGenericRepository<TForeignEntity> foreignRepo) 
            : base(foreignRepo.Database.UnitOfWork.TrackingStatusRepository, foreignRepo)
        {
        }

        /// <summary>
        /// Get all entities without a status
        /// </summary>
        /// <returns>IQueryable of entities without any status</returns>
        public IQueryable<TForeignEntity> GetEntitiesWithoutStatus(TrackingType type)
        {
            return GetEntitiesWithoutCustomFields(cf => cf.Type == type);
        }

        public IQueryable<TForeignEntity> GetUnprocessedEntities(TrackingType type)
        {
            //Get entities without a status, or with a status marked 'Unprocessed'
            return GetJoinResult(cf => cf.Type == type, null, jr => jr.CustomFieldDO == null || jr.CustomFieldDO.Status == TrackingStatus.UNPROCESSED).Select(jr => jr.ForeignDO);
        }

        /// <summary>
        /// Get all entities with a status confined to the provided predicate
        /// </summary>
        /// <returns>IQueryable of entities with a status confined to the provided predicate</returns>
        public IQueryable<TForeignEntity> GetEntitiesWhereTrackingStatus(Expression<Func<TrackingStatusDO, bool>> customFieldPredicate)
        {
            return GetEntitiesWithCustomField(customFieldPredicate);
        }

        /// <summary>
        /// Get all entities with a status
        /// </summary>
        /// <returns>IQueryable of entities with a status</returns>
        public IQueryable<TForeignEntity> GetEntitiesWithStatus(TrackingType type)
        {
            return GetEntitiesWithCustomField(cf => cf.Type == type);
        }

        /// <summary>
        /// Sets the status of an entity. If an existing status exists for the entity, the status will be updated. If not, a status will be created.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="entityDO">Entity to set the status on</param>
        /// <param name="status">Value of the status</param>
        public void SetStatus(TrackingType type, TForeignEntity entityDO, TrackingStatus status)
        {
            var row = GetOrCreateCustomField(entityDO, cf => cf.Type == type);
            row.Status = status;
            row.Type = type;
        }

        /// <summary>
        /// Gets the current status of an entity. If no status exists, null will be returned.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="entityDO">The status of the provided entity</param>
        public TrackingStatusDO GetStatus(TrackingType type, TForeignEntity entityDO)
        {
            return GetCustomField(entityDO, cf => cf.Type == type);
        }

        /// <summary>
        /// Deletes the status of an entity. If no status exists, no action will be performed.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="entityDO">Entity to delete the status on</param>
        public void DeleteStatus(TrackingType type, TForeignEntity entityDO)
        {
            DeleteCustomField(entityDO, cf => cf.Type == type);
        }
    }
}
