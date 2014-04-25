using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class TrackingStatusRepository : GenericRepository<TrackingStatusDO>, ITrackingStatusRepository
    {
        public TrackingStatusRepository(IUnitOfWork uow)
            : base(uow)
        {

        }

        public void Update()
        {
        }

        public IList<TForeignEntityType> GetForeignEntitiesWithoutStatus<TForeignEntityType>(IGenericRepository<TForeignEntityType> foreignRepository)
            where TForeignEntityType : class
        {
            return GetForeignEntities(foreignRepository, a => a.TrackingStatusDO == null);
        }

        public IList<TForeignEntityType> GetForeignEntitiesWithStatus<TForeignEntityType>(IGenericRepository<TForeignEntityType> foreignRepository, String status)
            where TForeignEntityType : class
        {
            return GetForeignEntities(foreignRepository, a => a.TrackingStatusDO != null && a.TrackingStatusDO.Value == status);
        }

        private IList<TForeignEntityType> GetForeignEntities<TForeignEntityType>(IGenericRepository<TForeignEntityType> foreignRepository, Func<JoinResult<TrackingStatusDO, TForeignEntityType>, bool> predicate)
            where TForeignEntityType : class
        {
            var ourQuery = GetQuery().DefaultIfEmpty();
            var foreignQuery = foreignRepository.GetQuery();

            return
                MakeJoin(ourQuery, foreignQuery)
                    .Cast<JoinResult<TrackingStatusDO, TForeignEntityType>>()
                    .Where(predicate)
                    .Select(a => a.ForeignDO)
                    .ToList();
        }

        private static IEnumerable MakeJoin<TOwnerEntityType, TForeignEntityType>(IQueryable<TOwnerEntityType> ourQuery, IQueryable<TForeignEntityType> foreignQuery)
            where TOwnerEntityType : class
            where TForeignEntityType : class
        {
            var ownerTableType = ourQuery.GetType().GenericTypeArguments.First();
            var foreignTableType = foreignQuery.GetType().GenericTypeArguments.First();

            var foreignTableKey = foreignTableType.GetProperties().First(p => p.CustomAttributes.Any(ca => ca.AttributeType == typeof (KeyAttribute)));

            Expression<Func<TrackingStatusDO, int>> trackingStatusKeySelector = ts => ts.ForeignTableID;
            Expression<Func<TForeignEntityType, IEnumerable<TOwnerEntityType>,JoinResult<TOwnerEntityType, TForeignEntityType>>> selector =
                                (owner, foreign) =>
                                    new JoinResult<TOwnerEntityType, TForeignEntityType>
                                    {
                                        ForeignDO = owner,
                                        TrackingStatusDO = foreign.FirstOrDefault()
                                    };

            var foreignProp = Expression.Parameter(foreignTableType);
            var propertyAccessor = Expression.Property(foreignProp, foreignTableKey);
            var foreignKeySelector = Expression.Lambda(propertyAccessor, new[] {foreignProp});

            var returnType = typeof(JoinResult<TOwnerEntityType, TForeignEntityType>);

            var ourCall =
                Expression.Call(
                    typeof (Queryable),
                    "GroupJoin",
                    new[] { foreignTableType, ownerTableType, foreignTableKey.PropertyType, returnType },
                    new Expression[]
                    {
                        Expression.Constant(foreignQuery),
                        Expression.Constant(ourQuery),
                        foreignKeySelector,
                        trackingStatusKeySelector,
                        selector
                    });

            var result = Expression.Lambda(ourCall).Compile().DynamicInvoke() as IEnumerable;
            if (result == null)
                throw new Exception("Unable to execute query.");

            return result;
        }

        public class JoinResult<TOwnerEntityType, TForeignEntityType>
             where TOwnerEntityType : class
             where TForeignEntityType : class
        {
            public TOwnerEntityType TrackingStatusDO;
            public TForeignEntityType ForeignDO;
        }
    }



    public interface ITrackingStatusRepository : IGenericRepository<TrackingStatusDO>
    {

    }
}