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
            return GetForeignEntitiesWhere(null, null, jr => jr.TrackingStatusDO == null);
        }

        public IQueryable<TForeignEntity> GetEntitiesWithStatus()
        {
            return GetForeignEntitiesWhere(null, null);
        }

        public IQueryable<TForeignEntity> GetEntitiesWhereTrackingStatus(Expression<Func<TrackingStatusDO, bool>> trackingStatusPredicate)
        {
            return GetForeignEntitiesWhere(trackingStatusPredicate, null);
        }

        private IQueryable<TForeignEntity> GetForeignEntitiesWhere(
            Expression<Func<TrackingStatusDO, bool>> trackingStatusPredicate, 
            Expression<Func<TForeignEntity, bool>> foreignEntityPredicate,
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
                    .Where(joinPredicate)
                    .Select(a => a.ForeignDO);
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
            var foreignTableType = typeof(TForeignEntity);
            var foreignTableKey = foreignTableType.GetProperties().First(p => p.CustomAttributes.Any(ca => ca.AttributeType == typeof(KeyAttribute)));

            var foreignProp = Expression.Parameter(foreignTableType);
            var propertyAccessor = Expression.Property(foreignProp, foreignTableKey);
            var foreignKeySelector = Expression.Lambda(propertyAccessor, new[] { foreignProp }) as Expression<Func<TForeignEntity, int>>;
            if(foreignKeySelector == null)
                throw new Exception("Query failed.");

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
        
        class JoinResult<TForeignEntityType>
            where TForeignEntityType : class
        {
            public TrackingStatusDO TrackingStatusDO;
            public TForeignEntityType ForeignDO;
        }
    }
}
