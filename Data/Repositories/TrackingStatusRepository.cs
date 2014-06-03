using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;

namespace Data.Repositories
{
    public class TrackingStatusRepository : GenericRepository<TrackingStatusDO>, ITrackingStatusRepository
    {
        internal TrackingStatusRepository(KwasanttDbContext KwasantDbContext)
            : base(KwasantDbContext)
        {

        }

        public void Update()
        {
        }
    }

    public interface ITrackingStatusRepository : IGenericRepository<TrackingStatusDO>
    {

    }
}