using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Data.Interfaces;
using StructureMap.TypeRules;

namespace KwasantCore.StructureMap
{
    public class MockedDBContext : IDBContext
    {
        private Dictionary<Type, object> _cachedSets = new Dictionary<Type, object>();
        public int SaveChanges()
        {
            while (AddForeignValues() > 0) ;

            return 1;
        }

        private int AddForeignValues()
        {
            var numAdded = 0;
            foreach (var set in _cachedSets.ToList())
            {
                foreach (var row in set.Value as IEnumerable)
                {
                    var props = row.GetType().GetProperties();
                    foreach (var prop in props)
                    {
                        if (IsEntity(prop.PropertyType))
                        {
                            //It's a normal foreign key
                            var value = prop.GetValue(row);
                            if (value == null)
                                continue;

                            if (AddValueToForeignSet(value))
                                numAdded++;
                        }
                        else if (prop.PropertyType.IsGenericType && prop.PropertyType.CanBeCastTo(typeof (IEnumerable<>)) &&
                                 IsEntity(prop.PropertyType.GetGenericArguments()[0]))
                        {
                            //It's a collection!
                            var collection = prop.GetValue(row) as IEnumerable;
                            if (collection == null)
                                continue;

                            foreach (var value in collection)
                            {
                                if (AddValueToForeignSet(value))
                                    numAdded++;
                            }
                        }
                    }
                }
            }
            return numAdded;
        }

        private bool AddValueToForeignSet(Object value)
        {
            var checkSet = Set(value.GetType());
            if ((checkSet as IEnumerable<object>).Contains(value))
            {
                return false;
            }

            var methodToCall = checkSet.GetType().GetMethod("Add", new[] {value.GetType()});
            methodToCall.Invoke(checkSet, new[] {value});
            return true;
        }

        public IDbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            var entityType = typeof (TEntity);
            return (IDbSet<TEntity>) (Set(entityType));
        }

        private object Set(Type entityType)
        {
            if (!_cachedSets.ContainsKey(entityType))
            {
                _cachedSets[entityType] = Activator.CreateInstance(typeof(MockedDbSet<>).MakeGenericType(entityType), new []{ this });
            }
            return _cachedSets[entityType];
        }

        public DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class
        {
            throw new System.NotImplementedException();
        }

        private bool IsEntity(Type type)
        {
            return type.Namespace == "Data.Entities";
        }
    }
}
