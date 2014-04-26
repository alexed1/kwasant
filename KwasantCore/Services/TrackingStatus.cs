using System;
using Data.Entities;
using Data.Interfaces;

namespace KwasantCore.Services
{
    /* Usage example
     
     * IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>();
     * TrackingStatusRepository trackingStatusRepo = new TrackingStatusRepository(uow);
     * EmailRepository emailRepo = new EmailRepository(uow);
     * 
     * TrackingStatus<EmailDO> ts = new TrackingStatus<EmailDO>(trackingStatusRepo, emailRepo);
     * 
     * ts.GetEntitiesWithoutStatus().ToList();
     * ts.GetEntitiesWhereTrackingStatus(trackingStatusDO => trackingStatusDO.Value == "ASD");
     * ts.GetEntitiesWithStatus().Where(emailDO => emailDO.Text == "Hello");
     * ts.GetEntitiesWithStatus();
     */
    /// <summary>
    /// This class is used to manage TrackingStatuses linked to Entities.
    /// It's a generic implementation, and as such, can be used with any entity in the database, so long as it has a single primary key. Composite keys are not supported.
    /// </summary>
    /// <typeparam name="TForeignEntity">The type of the linked entity (<see cref="EmailDO"></see>, for example)</typeparam>
    public class TrackingStatus<TForeignEntity> : GenericCustomField<TrackingStatusDO, TForeignEntity, String> 
        where TForeignEntity : class
    {
        public TrackingStatus(IGenericRepository<TrackingStatusDO> trackingStatusRepository, IGenericRepository<TForeignEntity> foreignRepo) 
            : base(trackingStatusRepository, foreignRepo)
        {
        }
    }
}
