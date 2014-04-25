using System;
using System.Collections;
using System.Collections.Generic;
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

        public IList<TForeignEntity> GetForeignEntitiesWithoutStatus()
        {
            return GetForeignEntitiesWhere(null, null, jr => jr.TrackingStatusDO == null);
        }

        public IList<TForeignEntity> GetForeignEntitiesWhereTrackingStatus(Expression<Func<TrackingStatusDO, bool>> trackingStatusPredicate)
        {
            return GetForeignEntitiesWhere(trackingStatusPredicate, null);
        }

        public IList<TForeignEntity> GetForeignEntitiesWhereForeignEntity(Expression<Func<TForeignEntity, bool>> foreignEntityPredicate)
        {
            return GetForeignEntitiesWhere(null, foreignEntityPredicate);
        }

        private IList<TForeignEntity> GetForeignEntitiesWhere(
            Expression<Func<TrackingStatusDO, bool>> trackingStatusPredicate, 
            Expression<Func<TForeignEntity, bool>> foreignEntityPredicate,
            Func<JoinResult<TForeignEntity>, bool> inMemoryPredicate = null)
        {
            if (trackingStatusPredicate == null)
                trackingStatusPredicate = trackingStatusDO => true;

            if (foreignEntityPredicate == null)
                foreignEntityPredicate = foreignEntityDO => true;

            if (inMemoryPredicate == null)
                inMemoryPredicate = jr => jr.TrackingStatusDO != null;

            var ourQuery = _trackingStatusRepository.GetQuery().Where(o => o.ForeignTableName == typeof(TForeignEntity).Name).Where(trackingStatusPredicate).DefaultIfEmpty();
            var foreignQuery = _foreignRepo.GetQuery().Where(foreignEntityPredicate);

            return
                MakeJoin(ourQuery, foreignQuery)
                    .Cast<JoinResult<TForeignEntity>>()
                    .Where(inMemoryPredicate)
                    .Select(a => a.ForeignDO)
                    .ToList();
        }

        //private IList<TForeignEntity> GetForeignEntities(Func<JoinResult<TForeignEntity>, bool> predicate)
        //{
        //    var ourQuery = _trackingStatusRepository.GetQuery().Where(o => o.ForeignTableName == typeof(TForeignEntity).Name).DefaultIfEmpty();
        //    var foreignQuery = _foreignRepo.GetQuery();

        //    return
        //        MakeJoin(ourQuery, foreignQuery)
        //            .Cast<JoinResult<TForeignEntity>>()
        //            .Where(predicate)
        //            .Select(a => a.ForeignDO)
        //            .ToList();
        //}

        /// <summary>
        /// This method is to generically generate a Queryable.Join() call on unknown types and properties.
        /// For example, we may want to join TrackingStatusDO -> EmailDO. The keys used will be (.ForeignTableID -> .EmailID)
        /// If however, we want to join TrackingStatusDO -> CustomerDO, the keys used will be (.ForeignTableID -> .CustomerID)
        /// Due to the fact that we don't use the same key name, we need to lookup the key using reflection.
        /// The below will generate code equivelant to (if joining to the EmailTable):
        /// emailRepo.GetQuery().Join
        /// (
        ///     trackingStatusRepo.GetQuery(),
        ///     (e) => e.EmailID,
        ///     (ts) => ts.ForeignTableID,
        ///     (e, ts) => new JoinResult { ForeignDO = e, TrackingStatusDO = ts.FirstOrDefault() }
        /// );
        /// 
        /// Note that emailRepo.GetQuery() is any IQueryable<EmailDO>, which means we _can_ have predicates, for example, querying emails sent by joesmith@gmail.com
        /// This also applies to the trackingStatusRepo.GetQuery().
        /// The below method is just a helper, and users can provide predicates using the above helpers
        ///  </summary>
        /// <typeparam name="TForeignEntityType"></typeparam>
        /// <param name="trackingStatusQuery"></param>
        /// <param name="foreignQuery"></param>
        /// <returns></returns>
        private static IEnumerable MakeJoin<TForeignEntityType>(IQueryable<TrackingStatusDO> trackingStatusQuery, IQueryable<TForeignEntityType> foreignQuery)
            where TForeignEntityType : class
        {
            var foreignTableType = foreignQuery.GetType().GenericTypeArguments.First();

            var foreignTableKey = foreignTableType.GetProperties().First(p => p.CustomAttributes.Any(ca => ca.AttributeType == typeof(KeyAttribute)));

            Expression<Func<TrackingStatusDO, int>> trackingStatusKeySelector = ts => ts.ForeignTableID;
            Expression<Func<TForeignEntityType, IEnumerable<TrackingStatusDO>, JoinResult<TForeignEntityType>>> selector =
                                (foreignDO, trackingStatusDO) =>
                                    new JoinResult<TForeignEntityType>
                                    {
                                        ForeignDO = foreignDO,
                                        TrackingStatusDO = trackingStatusDO.FirstOrDefault()
                                    };

            var foreignProp = Expression.Parameter(foreignTableType);
            var propertyAccessor = Expression.Property(foreignProp, foreignTableKey);
            var foreignKeySelector = Expression.Lambda(propertyAccessor, new[] { foreignProp });

            var returnType = typeof(JoinResult<TForeignEntityType>);
            var ourCall =
                Expression.Call(
                    typeof(Queryable),
                    "GroupJoin",
                    new[] { foreignTableType, typeof(TrackingStatusDO), foreignTableKey.PropertyType, returnType },
                    new Expression[]
                    {
                        Expression.Constant(foreignQuery),
                        Expression.Constant(trackingStatusQuery),
                        foreignKeySelector,
                        trackingStatusKeySelector,
                        selector
                    });

            return Expression.Lambda(ourCall).Compile().DynamicInvoke() as IEnumerable;
        }
        
        class JoinResult<TForeignEntityType>
            where TForeignEntityType : class
        {
            public TrackingStatusDO TrackingStatusDO;
            public TForeignEntityType ForeignDO;
        }
    }
}
