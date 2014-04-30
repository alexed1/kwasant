using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Data.Interfaces;

namespace KwasantCore.StructureMap
{
    public class MockedDbSet<TEntity> : IDbSet<TEntity>
        where TEntity : class
    {
        private readonly IDBContext _dbContext;
        private HashSet<TEntity> _set = new HashSet<TEntity>();

        public MockedDbSet(IDBContext dbContext)
        {
            _dbContext = dbContext;
            _set = new HashSet<TEntity>();
        }

        public IEnumerator<TEntity> GetEnumerator()
        {
            return _set.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Expression Expression
        {
            get
            {
                return _set.AsQueryable().Expression;
            }
            private set { }
        }

        public Type ElementType
        {
            get
            {
                return _set.AsQueryable().ElementType;
            }
            private set { }
        }
        public IQueryProvider Provider
        {
            get
            {
                return _set.AsQueryable().Provider;
            }
            private set { }
        }
        public TEntity Find(params object[] keyValues)
        {
            if (keyValues.Length == 0)
                throw new Exception("No primary key found on " + EntityName);
            if (keyValues.Length > 1)
                throw new Exception("Multiple keys found on " + EntityName + ". Only singular keys are supported");
            if (!(keyValues[0] is int))
                throw new Exception("Only support int-based keys.");

            var entityPrimaryKey = (int) keyValues[0];

            var compiledSelector = GetEntityKeySelector().Compile();
            return _set.FirstOrDefault(r => compiledSelector(r) == entityPrimaryKey);
        }

        public TEntity Add(TEntity entity)
        {
            _set.Add(entity);
            return entity;
        }

        public TEntity Remove(TEntity entity)
        {
            _set.Remove(entity);
            return entity;
        }

        public TEntity Attach(TEntity entity)
        {
            return entity;
        }

        public TEntity Create()
        {
            throw new Exception("Not supported yet!");
        }

        public TDerivedEntity Create<TDerivedEntity>() where TDerivedEntity : class, TEntity
        {
            throw new Exception("Not supported yet!");
        }

        public ObservableCollection<TEntity> Local
        {
            get
            {
                return null;
            }
            private set { }
        }



        protected Expression<Func<TEntity, int>> GetEntityKeySelector()
        {
            Type entityType = typeof(TEntity);
            PropertyInfo tableKey = EntityPrimaryKeyPropertyInfo;

            //The following three lines generates the following in LINQ syntax:
            // (e) => e.[PrimaryKeyProperty]; - where PrimaryKeyProperty is the primary key of the entity
            // Example of EmailDO:
            // (e) => e.EmailID;
            ParameterExpression proe = Expression.Parameter(entityType);
            MemberExpression propertyAccessor = Expression.Property(proe, tableKey);
            Expression<Func<TEntity, int>> entityKeySelector = Expression.Lambda(propertyAccessor, new[] { proe }) as Expression<Func<TEntity, int>>;
            return entityKeySelector;
        }

        private String _entityName;
        public String EntityName
        {
            get
            {
                return _entityName ?? (_entityName = typeof(TEntity).Name);
            }
        }

        private PropertyInfo _entityPrimaryKeyPropertyInfo;
        public PropertyInfo EntityPrimaryKeyPropertyInfo
        {
            get
            {
                if (_entityPrimaryKeyPropertyInfo == null)
                {
                    List<PropertyInfo> keys = typeof(TEntity).GetProperties().Where(p => p.GetCustomAttributes(typeof(KeyAttribute), true).Any()).ToList();
                    if (keys.Count > 1)
                        throw new Exception("Linked entity MUST have a single primary key. Composite keys are not supported.");
                    //If no primary key exists, we cannot use it
                    if (keys.Count == 0)
                        throw new Exception(
                            "Linked entity MUST have a single primary key. Entities without primary keys are not supported.");

                    _entityPrimaryKeyPropertyInfo = keys.First();
                }
                return _entityPrimaryKeyPropertyInfo;
            }
        }

    }
}
