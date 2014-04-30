using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using Data.Interfaces;
using StructureMap.TypeRules;

namespace KwasantCore.StructureMap
{
    public class MockedDBContext : IDBContext
    {
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

            return 1;
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

        private bool IsEntity(Type type)
        {
            return type.Namespace == "Data.Entities";
        }
    }
}
