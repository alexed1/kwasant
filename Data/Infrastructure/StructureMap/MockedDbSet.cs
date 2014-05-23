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

namespace Data.Infrastructure.StructureMap
{
    public class MockedDbSet<TEntityType> : IDbSet<TEntityType>
        where TEntityType : class
    {
        private readonly IDBContext _dbContext;
        private HashSet<TEntityType> _set = new HashSet<TEntityType>();

        public MockedDbSet(IDBContext dbContext)
        {
            _dbContext = dbContext;
            _set = new HashSet<TEntityType>();
        }

        public IEnumerator<TEntityType> GetEnumerator()
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
        public TEntityType Find(params object[] keyValues)
        {
            if (keyValues.Length == 0)
                throw new Exception("No primary key found on " + EntityName);
            if (keyValues.Length > 1)
                throw new Exception("Multiple keys found on " + EntityName + ". Only singular keys are supported");

            var keyType = keyValues[0].GetType();
            if (keyType != typeof(int) && keyType != typeof(String))
                throw new Exception("Only supports int-based or string-based keys.");

            if (keyType == typeof (String))
            {
                string entityPrimaryKey = keyValues[0] as string;
                Func<TEntityType, string> compiledSelector = GetEntityKeySelectorString().Compile();
                return _set.FirstOrDefault(r => compiledSelector(r) == entityPrimaryKey);    
            }
            else
            {
                
                int entityPrimaryKey = (int)(keyValues[0]);
                Func<TEntityType, int> compiledSelector = GetEntityKeySelectorInt().Compile();
                return _set.FirstOrDefault(r => compiledSelector(r) == entityPrimaryKey);
            }

            
        }

        public TEntityType Add(TEntityType entity)
        {
            _set.Add(entity);
            return entity;
        }

        public TEntityType Remove(TEntityType entity)
        {
            _set.Remove(entity);
            return entity;
        }

        public TEntityType Attach(TEntityType entity)
        {
            return entity;
        }

        public TEntityType Create()
        {
            throw new Exception("Not supported yet!");
        }

        public TDerivedEntity Create<TDerivedEntity>() where TDerivedEntity : class, TEntityType
        {
            throw new Exception("Not supported yet!");
        }

        public ObservableCollection<TEntityType> Local
        {
            get
            {
                return new ObservableCollection<TEntityType>(this);
            }
            private set { }
        }



        protected Expression<Func<TEntityType, int>> GetEntityKeySelectorInt()
        {
            Type entityType = typeof(TEntityType);
            PropertyInfo tableKey = EntityPrimaryKeyPropertyInfo;

            //The following three lines generates the following in LINQ syntax:
            // (e) => e.[PrimaryKeyProperty]; - where PrimaryKeyProperty is the primary key of the entity
            // Example of EmailDO:
            // (e) => e.EmailID;
            ParameterExpression proe = Expression.Parameter(entityType);
            MemberExpression propertyAccessor = Expression.Property(proe, tableKey);
            Expression<Func<TEntityType, int>> entityKeySelector = Expression.Lambda(propertyAccessor, new[] { proe }) as Expression<Func<TEntityType, int>>;
            return entityKeySelector;
        }

        protected Expression<Func<TEntityType, String>> GetEntityKeySelectorString()
        {
            Type entityType = typeof(TEntityType);
            PropertyInfo tableKey = EntityPrimaryKeyPropertyInfo;

            //The following three lines generates the following in LINQ syntax:
            // (e) => e.[PrimaryKeyProperty]; - where PrimaryKeyProperty is the primary key of the entity
            // Example of EmailDO:
            // (e) => e.EmailID;
            ParameterExpression proe = Expression.Parameter(entityType);
            MemberExpression propertyAccessor = Expression.Property(proe, tableKey);
            Expression<Func<TEntityType, String>> entityKeySelector = Expression.Lambda(propertyAccessor, new[] { proe }) as Expression<Func<TEntityType, String>>;
            return entityKeySelector;
        }

        private String _entityName;
        public String EntityName
        {
            get
            {
                return _entityName ?? (_entityName = typeof(TEntityType).Name);
            }
        }

        private PropertyInfo _entityPrimaryKeyPropertyInfo;
        public PropertyInfo EntityPrimaryKeyPropertyInfo
        {
            get
            {
                if (_entityPrimaryKeyPropertyInfo == null)
                {
                    List<PropertyInfo> keys = typeof(TEntityType).GetProperties().Where(p => p.GetCustomAttributes(typeof(KeyAttribute), true).Any()).ToList();
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

        public void AddOrUpdate(
            Expression<Func<TEntityType, Object>> identifierExpression,
            params TEntityType[] entities
            )
        {
            var lambda = identifierExpression.Compile();
            foreach (var entity in entities)
            {
                if (!this.Any(dbEntity => lambda(dbEntity).Equals(lambda(entity))))
                {
                    Add(entity);
                }
            }
        }


    }
}
