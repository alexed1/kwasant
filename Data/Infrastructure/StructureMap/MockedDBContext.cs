using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using Data.Interfaces;
using Data.Migrations;
using StructureMap.TypeRules;

namespace Data.Infrastructure.StructureMap
{
    public class MockedDBContext : IDBContext
    {
        public MockedDBContext()
        {
            MigrationConfiguration.Seed(new UnitOfWork(this));
        }

        private Dictionary<Type, object> _cachedSets = new Dictionary<Type, object>();
        public int SaveChanges()
        {
            //When we save in memory, we need to make sure foreign entities are saved. An example:
            //eventDO.Emails.Add(new EmailDO();
            //This new EmailDO is added to the event's Email collection - however, it's not automatically added to a set.
            //When we save, we parse all the row's foreign links, including collections. We then add missing rows to the collection
            //We loop, because in this instance:
            //eventDO.Emails.Add(new EmailDO { Customer = new CustomerDO }) - once we've added the email, we still haven't added the customer.
            //Looping causes us to pickup each foreign link and be sure everything is persisted in memory
            while (AddForeignValues() > 0) ;

            AssignIDs();

            return 1;
        }

        private void AssignIDs()
        {
            foreach (var set in _cachedSets)
            {
                int maxIDAlready = 0;
                if ((set.Value as IEnumerable<object>).Any())
                {
                    maxIDAlready = (set.Value as IEnumerable<object>).Max<object, int>(a =>
                    {
                        var propInfo = EntityPrimaryKeyPropertyInfo(a);
                        if (propInfo == null)
                            return 0;

                        return (int) propInfo.GetValue(a);
                    });
                }

                foreach (object row in set.Value as IEnumerable)
                {
                    var propInfo = EntityPrimaryKeyPropertyInfo(row);
                    if (propInfo == null)
                        continue;

                    if ((int) propInfo.GetValue(row) == 0)
                    {
                        propInfo.SetValue(row, ++maxIDAlready);
                    }
                }
            }
        }

        private int AddForeignValues()
        {
            int numAdded = 0;
            foreach (KeyValuePair<Type, object> set in _cachedSets.ToList())
            {
                foreach (object row in set.Value as IEnumerable)
                {
                    PropertyInfo[] props = row.GetType().GetProperties();
                    foreach (PropertyInfo prop in props)
                    {
                        if (IsEntity(prop.PropertyType))
                        {
                            //It's a normal foreign key
                            object value = prop.GetValue(row);
                            if (value == null)
                                continue;

                            if (AddValueToForeignSet(value))
                                numAdded++;
                        }
                        else if (prop.PropertyType.IsGenericType && prop.PropertyType.CanBeCastTo(typeof (IEnumerable<>)) &&
                                 IsEntity(prop.PropertyType.GetGenericArguments()[0]))
                        {
                            //It's a collection!
                            IEnumerable collection = prop.GetValue(row) as IEnumerable;
                            if (collection == null)
                                continue;

                            foreach (object value in collection)
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
            object checkSet = Set(value.GetType());
            if ((checkSet as IEnumerable<object>).Contains(value))
            {
                return false;
            }

            MethodInfo methodToCall = checkSet.GetType().GetMethod("Add", new[] {value.GetType()});
            methodToCall.Invoke(checkSet, new[] {value});
            return true;
        }

        public IDbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            Type entityType = typeof (TEntity);
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

        public IUnitOfWork UnitOfWork { get; set; }

        private bool IsEntity(Type type)
        {
            return type.Namespace == "Data.Entities";
        }

        public PropertyInfo EntityPrimaryKeyPropertyInfo(object entity)
        {
            var entityType = entity.GetType();
            List<PropertyInfo> keys = entityType.GetProperties().Where(p => p.GetCustomAttributes(typeof (KeyAttribute), true).Any()).ToList();
            if (keys.Count > 1)
                return null;
            //If no primary key exists, we cannot use it
            if (keys.Count == 0)
                return null;

            return keys.First();
        }

        public void Dispose()
        {
            
        }
    }
}
