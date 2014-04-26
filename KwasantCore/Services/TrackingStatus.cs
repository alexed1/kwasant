using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;

namespace KwasantCore.Services
{
    public class TrackingStatus<TForeignEntity>
        where TForeignEntity : class
    {
        private readonly ITrackingStatusRepository _trackingStatusRepository;
        private readonly IGenericRepository<TForeignEntity> _foreignRepo;

        public TrackingStatus(ITrackingStatusRepository trackingStatusRepository,
            IGenericRepository<TForeignEntity> foreignRepo)
        {
            _trackingStatusRepository = trackingStatusRepository;
            _foreignRepo = foreignRepo;
        }

        public IQueryable<TForeignEntity> GetEntitiesWithoutStatus()
        {
            return GetEntities(null, null, jr => jr.TrackingStatusDO == null);
        }

        public IQueryable<TForeignEntity> GetEntitiesWithStatus()
        {
            return GetEntities();
        }

        public IQueryable<TForeignEntity> GetEntitiesWhereTrackingStatus(Expression<Func<TrackingStatusDO, bool>> trackingStatusPredicate)
        {
            return GetEntities(trackingStatusPredicate);
        }

        public void SetStatus(TForeignEntity entityDO, String status)
        {
            SetStatus(GetKey(entityDO), status);
        }

        private void SetStatus(int entityID, String status)
        {
            var currentStatus = GetStatus(entityID);
            if (currentStatus == null)
            {
                currentStatus = new TrackingStatusDO
                {
                    ForeignTableID = entityID,
                    ForeignTableName = typeof(TForeignEntity).Name,
                };
                _trackingStatusRepository.Add(currentStatus);
            }
            currentStatus.Value = status;
        }

        public void DeleteStatus(TForeignEntity entityDO)
        {
            DeleteStatus(GetKey(entityDO));
        }

        private void DeleteStatus(int entityID)
        {
            var currentStatus = GetStatus(entityID);
            if (currentStatus != null)
            {
                _trackingStatusRepository.Remove(currentStatus);
            }
        }

        public TrackingStatusDO GetStatus(TForeignEntity entity)
        {
            var inMemoryID = GetKey(entity);
            return GetStatus(inMemoryID);
        }

        private TrackingStatusDO GetStatus(int entityEntityID)
        {
            var foreignTableType = typeof(TForeignEntity);
            var foreignTableKey = foreignTableType.GetProperties().First(p => p.CustomAttributes.Any(ca => ca.AttributeType == typeof(KeyAttribute)));

            var foreignProp = Expression.Parameter(foreignTableType);
            var propertyAccessor = Expression.Property(foreignProp, foreignTableKey);

            var equalExpression = Expression.Equal(propertyAccessor, Expression.Constant(entityEntityID));
            var foreignKeyComparer = Expression.Lambda(equalExpression, new[] { foreignProp }) as Expression<Func<TForeignEntity, bool>>;

            return GetJoinResult(null, foreignKeyComparer).Select(jr => jr.TrackingStatusDO).FirstOrDefault();
        }


        private IQueryable<TForeignEntity> GetEntities(
            Expression<Func<TrackingStatusDO, bool>> trackingStatusPredicate = null, 
            Expression<Func<TForeignEntity, bool>> foreignEntityPredicate = null,
            Expression<Func<JoinResult<TForeignEntity>, bool>> joinPredicate = null)
        {
            return GetJoinResult(trackingStatusPredicate, foreignEntityPredicate, joinPredicate)
                .Select(a => a.ForeignDO);
        }

        private IQueryable<JoinResult<TForeignEntity>> GetJoinResult(
            Expression<Func<TrackingStatusDO, bool>> trackingStatusPredicate = null,
            Expression<Func<TForeignEntity, bool>> foreignEntityPredicate = null,
            Expression<Func<JoinResult<TForeignEntity>, bool>> joinPredicate = null)
        {
            if (trackingStatusPredicate == null)
                trackingStatusPredicate = trackingStatusDO => true;

            if (foreignEntityPredicate == null)
                foreignEntityPredicate = foreignEntityDO => true;

            if (joinPredicate == null)
                joinPredicate = jr => jr.TrackingStatusDO != null;

            var ourQuery = _trackingStatusRepository.GetQuery().Where(o => o.ForeignTableName == typeof(TForeignEntity).Name).Where(trackingStatusPredicate).DefaultIfEmpty();
            var foreignQuery = _foreignRepo.GetQuery().Where(foreignEntityPredicate);


            return
                MakeJoin(ourQuery, foreignQuery)
                    .Where(joinPredicate);
        }

        /// <summary>
        /// This method is to generically generate a Queryable.Join() call on unknown types and properties.
        /// For example, we may want to join TrackingStatusDO -> EmailDO. The keys used will be (.ForeignTableID -> .EmailID)
        /// If however, we want to join TrackingStatusDO -> CustomerDO, the keys used will be (.ForeignTableID -> .CustomerID)
        /// Due to the fact that we don't use the same key name, we need to lookup the key using reflection.
        /// The below will generate code equivelant to (if joining to the EmailTable):
        /// emailRepo.GetQuery().GroupJoin
        /// (
        ///     trackingStatusRepo.GetQuery(),
        ///     (e) => e.EmailID,
        ///     (ts) => ts.ForeignTableID,
        ///     (e, ts) => new JoinResult { ForeignDO = e, TrackingStatusDO = ts.FirstOrDefault() }
        /// );
        /// 
        /// Note that emailRepo.GetQuery() is any IQueryable, which means we _can_ have predicates, for example, querying emails sent by joesmith@gmail.com
        /// This also applies to the trackingStatusRepo.GetQuery().
        /// The below method is just a helper, and users can provide predicates using the above methods
        /// </summary>
        /// <typeparam name="TForeignEntity"></typeparam>
        /// <param name="trackingStatusQuery"></param>
        /// <param name="foreignQuery"></param>
        /// <returns></returns>
        private static IQueryable<JoinResult<TForeignEntity>> MakeJoin(IQueryable<TrackingStatusDO> trackingStatusQuery, IQueryable<TForeignEntity> foreignQuery)
        {
            var foreignKeySelector = GetForeignKeySelectorExpression();

            return foreignQuery.GroupJoin
                (
                    trackingStatusQuery,
                    foreignKeySelector,
                    ts => ts.ForeignTableID,
                    (foreignDO, trackingStatusDO) =>
                        new JoinResult<TForeignEntity>
                        {
                            ForeignDO = foreignDO,
                            TrackingStatusDO = trackingStatusDO.FirstOrDefault()
                        }
                );
        }

        private static int GetKey(TForeignEntity entity)
        {
            return GetForeignKeySelectorExpression().Compile().Invoke(entity);
        }
       
        private static Expression<Func<TForeignEntity, int>> GetForeignKeySelectorExpression()
        {
            var foreignTableType = typeof (TForeignEntity);
            var foreignTableKey =
                foreignTableType.GetProperties()
                    .First(p => p.CustomAttributes.Any(ca => ca.AttributeType == typeof (KeyAttribute)));

            var foreignProp = Expression.Parameter(foreignTableType);
            var propertyAccessor = Expression.Property(foreignProp, foreignTableKey);
            var foreignKeySelector = Expression.Lambda(propertyAccessor, new[] {foreignProp}) as Expression<Func<TForeignEntity, int>>;
            return foreignKeySelector;
        }

        class JoinResult<TForeignEntityType>
            where TForeignEntityType : class
        {
            public TrackingStatusDO TrackingStatusDO;
            public TForeignEntityType ForeignDO;
        }
    }
}
