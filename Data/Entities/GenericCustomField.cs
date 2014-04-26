using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Data.Interfaces;

namespace Data.Entities
{
    /// <summary>
    /// This class is used to manage CustomFields linked to Entities. See KwasantCore.Services.TrackingStatus for more information.
    /// It's a generic implementation, and as such, can be used with any entity in the database, so long as it has a single primary key. Composite keys are not supported.
    /// </summary>
    /// <typeparam name="TForeignEntity">The type of the linked entity (<see cref="EmailDO"></see>, for example)</typeparam>
    /// <typeparam name="TCustomFieldType">The type of the custom field (<see cref="TrackingStatusDO"></see> for example</typeparam>
    /// <typeparam name="TCustomFieldValueType">The type of the custom field value </typeparam>
    public class GenericCustomField<TCustomFieldType, TForeignEntity, TCustomFieldValueType>
        where TCustomFieldType : class, ICustomField<TCustomFieldValueType>, new()
        where TForeignEntity : class
    {
        private readonly IGenericRepository<TCustomFieldType> _trackingStatusRepo;
        private readonly IGenericRepository<TForeignEntity> _foreignRepo;

        public GenericCustomField(IGenericRepository<TCustomFieldType> trackingStatusRepo,
            IGenericRepository<TForeignEntity> foreignRepo)
        {
            _trackingStatusRepo = trackingStatusRepo;
            _foreignRepo = foreignRepo;
        }

        /// <summary>
        /// Get all entities without a custom field
        /// </summary>
        /// <returns>IQueryable of entities without any custom field</returns>
        protected IQueryable<TForeignEntity> GetEntitiesWithoutCustomFields()
        {
            return GetEntities(null, null, jr => jr.CustomFieldDO == null);
        }

        /// <summary>
        /// Get all entities with a custom field
        /// </summary>
        /// <returns>IQueryable of entities with a custom field</returns>
        protected IQueryable<TForeignEntity> GetEntitiesWithCustomField()
        {
            return GetEntities();
        }

        /// <summary>
        /// Get all entities with a custom field confined to the provided predicate
        /// </summary>
        /// <returns>IQueryable of entities with a custom field confined to the provided predicate</returns>
        protected IQueryable<TForeignEntity> GetEntitiesWhereCustomField(Expression<Func<TCustomFieldType, bool>> customFieldPredicate)
        {
            return GetEntities(customFieldPredicate);
        }

        /// <summary>
        /// Sets the custom field of an entity. If an existing custom field exists for the entity, the status will be updated. If not, a custom field will be created.
        /// Entities _MUST_ already exist in the database.
        /// </summary>
        /// <param name="entityDO">Entity to set the custom field on</param>
        /// <param name="status">Value of the custom field</param>
        protected void SetCustomField(TForeignEntity entityDO, TCustomFieldValueType status)
        {
            SetCustomField(GetKey(entityDO), status);
        }

        protected void SetCustomField(int entityID, TCustomFieldValueType status)
        {
            if(entityID == 0)
                throw new Exception("Cannot be applied to new entities. Entities must be saved to the database before applying a custom field.");
            var currentStatus = GetCustomField(entityID);
            if (currentStatus == null)
            {
                currentStatus = new TCustomFieldType
                {
                    ForeignTableID = entityID,
                    ForeignTableName = typeof(TForeignEntity).Name,
                };
                _trackingStatusRepo.Add(currentStatus);
            }
            currentStatus.Value = status;
        }

        /// <summary>
        /// Deletes the custom field of an entity. If no custom field exists, no action will be performed.
        /// </summary>
        /// <param name="entityDO">Entity to delete the custom field on</param>
        protected void DeleteCustomField(TForeignEntity entityDO)
        {
            DeleteCustomField(GetKey(entityDO));
        }

        protected void DeleteCustomField(int entityID)
        {
            var currentStatus = GetCustomField(entityID);
            if (currentStatus != null)
            {
                _trackingStatusRepo.Remove(currentStatus);
            }
        }

        /// <summary>
        /// Gets the current custom field of an entity. If no status exists, null will be returned.
        /// </summary>
        /// <param name="entityDO">The custom field of the provided entity</param>
        protected TCustomFieldType GetCustomField(TForeignEntity entityDO)
        {
            var inMemoryID = GetKey(entityDO);
            return GetCustomField(inMemoryID);
        }

        protected TCustomFieldType GetCustomField(int entityID)
        {
            //This effectively builds a lambda as follows:
            // e => e.[PrimaryKeyProperty] == entityID

            var foreignTableType = typeof(TForeignEntity);
            var foreignTableKey = GetForeignTableKeyPropertyInfo(foreignTableType);

            var foreignProp = Expression.Parameter(foreignTableType);
            var propertyAccessor = Expression.Property(foreignProp, foreignTableKey);

            var equalExpression = Expression.Equal(propertyAccessor, Expression.Constant(entityID));
            var foreignKeyComparer = Expression.Lambda(equalExpression, new[] { foreignProp }) as Expression<Func<TForeignEntity, bool>>;

            return GetJoinResult(null, foreignKeyComparer).Select(jr => jr.CustomFieldDO).FirstOrDefault();
        }


        protected IQueryable<TForeignEntity> GetEntities(
            Expression<Func<TCustomFieldType, bool>> customFieldStatus = null,
            Expression<Func<TForeignEntity, bool>> foreignEntityPredicate = null,
            Expression<Func<JoinResult, bool>> joinPredicate = null)
        {
            return GetJoinResult(customFieldStatus, foreignEntityPredicate, joinPredicate)
                .Select(a => a.ForeignDO);
        }

        protected IQueryable<JoinResult> GetJoinResult(
            Expression<Func<TCustomFieldType, bool>> customFieldStatus = null,
            Expression<Func<TForeignEntity, bool>> foreignEntityPredicate = null,
            Expression<Func<JoinResult, bool>> joinPredicate = null)
        {
            //If we don't have a predicate on the tracking status, set it to always true (Linq2SQL removes the redundant 'return true' predicate, so no performance is lost)
            if (customFieldStatus == null)
                customFieldStatus = customFieldDO => true;

            //If we don't have a predicate on the foreign entity, set it to always true (Linq2SQL removes the redundant 'return true' predicate, so no performance is lost)
            if (foreignEntityPredicate == null)
                foreignEntityPredicate = foreignEntityDO => true;

            //By default, we only return entities who have a custom field. The exception to this is GetEntitiesWithoutStatus, which provides its own join predicate
            if (joinPredicate == null)
                joinPredicate = jr => jr.CustomFieldDO != null;

            // 1. Make sure we join to the table name (otherwise we'll get incorrect entities).
            // 2. Provide our tracking status predicate
            // 3. DefaultIfEmpty() turns the query into a left join, rather than inner (since we do sometimes want to return entities without statuses).
            var ourQuery = _trackingStatusRepo.GetQuery().Where(o => o.ForeignTableName == typeof(TForeignEntity).Name).Where(customFieldStatus).DefaultIfEmpty();

            //Apply our foreign entity predicate
            var foreignQuery = _foreignRepo.GetQuery().Where(foreignEntityPredicate);

            //Make the join and apply our join predicate
            return
                MakeJoin(ourQuery, foreignQuery)
                    .Where(joinPredicate);
        }

        /// <summary>
        /// This method is to generically generate a Queryable.Join() call on unknown types and properties.
        /// For example, we may want to join TCustomFieldType -> EmailDO. The keys used will be (.ForeignTableID -> .EmailID)
        /// If however, we want to join TCustomFieldType -> CustomerDO, the keys used will be (.ForeignTableID -> .CustomerID)
        /// Due to the fact that we don't use the same key name, we need to lookup the key using reflection.
        /// The below will generate code equivelant to (if joining to the EmailTable):
        /// emailRepo.GetQuery().GroupJoin
        /// (
        ///     customFieldRepo.GetQuery(),
        ///     (e) => e.EmailID,
        ///     (ts) => ts.ForeignTableID,
        ///     (e, ts) => new JoinResult { ForeignDO = e, TCustomFieldType = ts.FirstOrDefault() }
        /// );
        /// 
        /// Note that emailRepo.GetQuery() is any IQueryable, which means we _can_ have predicates, for example, querying emails sent by joesmith@gmail.com
        /// This also applies to customFieldQuery.
        /// The below method is just a helper, and users can provide predicates using the above methods
        /// </summary>
        /// <typeparam name="TForeignEntity"></typeparam>
        /// <param name="customFieldQuery"></param>
        /// <param name="foreignQuery"></param>
        /// <returns></returns>
        protected static IQueryable<JoinResult> MakeJoin(IQueryable<TCustomFieldType> customFieldQuery, IQueryable<TForeignEntity> foreignQuery)
        {
            //Grab our foreign key selector (in the form of (e) => e.[PrimaryKeyProperty]) - where PrimaryKeyProperty is the primary key of the entity
            var foreignKeySelector = GetForeignKeySelectorExpression();

            //Make the join!
            return foreignQuery.GroupJoin
                (
                    customFieldQuery,
                    foreignKeySelector,
                    ts => ts.ForeignTableID,
                    (foreignDO, customFieldDO) =>
                        new JoinResult
                        {
                            ForeignDO = foreignDO,
                            CustomFieldDO = customFieldDO.FirstOrDefault()
                        }
                );
        }

        /// <summary>
        /// This method returns the primary key of the provided entity. Retrieves value based on the first property with the [Key] attribute.
        /// </summary>
        protected static int GetKey(TForeignEntity entity)
        {
            return GetForeignKeySelectorExpression().Compile().Invoke(entity);
        }

        /// <summary>
        /// This method returns an expression which retrives the primary key of an entity. This is not executed immediately. When passed to Linq2SQL, it is translated into a SQL call (not in memory).
        /// </summary>
        protected static Expression<Func<TForeignEntity, int>> GetForeignKeySelectorExpression()
        {
            var foreignTableType = typeof(TForeignEntity);
            var foreignTableKey = GetForeignTableKeyPropertyInfo(foreignTableType);

            //The following three lines generates the following in LINQ syntax:
            // (e) => e.[PrimaryKeyProperty]; - where PrimaryKeyProperty is the primary key of the entity
            // Example of EmailDO:
            // (e) => e.EmailID;
            var foreignProp = Expression.Parameter(foreignTableType);
            var propertyAccessor = Expression.Property(foreignProp, foreignTableKey);
            var foreignKeySelector = Expression.Lambda(propertyAccessor, new[] { foreignProp }) as Expression<Func<TForeignEntity, int>>;
            return foreignKeySelector;
        }

        protected static PropertyInfo GetForeignTableKeyPropertyInfo(Type foreignTableType)
        {
            //Get the first primary key we see on the entity
            var foreignTableKeys =
                foreignTableType.GetProperties()
                    .Where(p => p.CustomAttributes.Any(ca => ca.AttributeType == typeof(KeyAttribute)))
                    .ToList();
            if (foreignTableKeys.Count > 1)
                throw new Exception("Linked entity MUST have a single primary key. Composite keys are not supported.");
            //If no primary key exists, we cannot use it
            if (foreignTableKeys.Count == 0)
                throw new Exception(
                    "Linked entity MUST have a single primary key. Entities without primary keys are not supported.");

            return foreignTableKeys.First();
        }

        /// <summary>
        /// Class used to store the join result. We cannot use anonymous methods as typical in Linq2SQL, as we return the join, rather than immediately process it.
        /// </summary>
        protected class JoinResult
        {
            public TCustomFieldType CustomFieldDO;
            public TForeignEntity ForeignDO;
        }
    }
}
