using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using Data.Entities;
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

        private readonly Dictionary<Type, IEnumerable<object>> _cachedSets = new Dictionary<Type, IEnumerable<object>>();
        private object[] _addedEntities;
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

            DetectChanges();

            AssignIDs();

            AssertConstraints();
            
            foreach (var newBookingRequestDO in _addedEntities.OfType<BookingRequestDO>())
                AlertManager.BookingRequestCreated(newBookingRequestDO.Id);
            
            return 1;
        }

        public void DetectChanges()
        {
            _addedEntities = GetAdds().ToArray();
        }

        public object[] AddedEntities
        {
            get { return _addedEntities; }
        }

        public object[] ModifiedEntities
        {
            get
            {
                // TODO: not supported currenty. May be implemented via hashcodes.
                return new object[0];
            }
        }

        public object[] DeletedEntities
        {
            get
            {
                // TODO: not supported currenty.
                return new object[0];
            }
        }

        private void AssertConstraints()
        {
            foreach (var set in _cachedSets)
            {
                foreach (object row in set.Value)
                {
                    //Check nullable constraint enforced
                    foreach (var prop in row.GetType().GetProperties())
                    {
                        var hasAttribute = prop.GetCustomAttributes(typeof (RequiredAttribute)).Any();
                        if (hasAttribute)
                        {
                            if(prop.GetValue(row) == null)
                                throw new Exception("Property '" + prop.Name + "' on '" + row.GetType().Name + "' is marked as required, but is being saved with a null value.");
                        }
                    }
                }
            }
        }

        private IEnumerable<object> GetAdds()
        {
            foreach (var set in _cachedSets)
            {
                foreach (object row in set.Value as IEnumerable)
                {
                    var propInfo = EntityPrimaryKeyPropertyInfo(row);
                    if (propInfo == null)
                        continue;

                    if ((int) propInfo.GetValue(row) == 0)
                    {
                        yield return row;
                    }
                }
            }
        }

/*
        public List<KwasantDbContext.PropertyChangeInformation> GetEntityModifications<T>(T entity) where T : class
        {
            throw new NotImplementedException();
        }

        public List<KwasantDbContext.EntityChangeInformation> GetModifiedEntities()
        {
            throw new NotImplementedException();
        }

*/
        private void AssignIDs()
        {
            foreach (var set in _cachedSets)
            {
                int maxIDAlready = 0;
                if (set.Value.Any())
                {
                    maxIDAlready = set.Value.Max<object, int>(a =>
                    {
                        var propInfo = EntityPrimaryKeyPropertyInfo(a);
                        if (propInfo == null)
                            return 0;

                        return (int)propInfo.GetValue(a);
                    });
                }

                foreach (var row in set.Value)
                {
                    var propInfo = EntityPrimaryKeyPropertyInfo(row);
                    if (propInfo == null)
                        continue;

                    if ((int)propInfo.GetValue(row) == 0)
                    {
                        propInfo.SetValue(row, ++maxIDAlready);
                    }
                }
            }
        }

        private int AddForeignValues()
        {
            int numAdded = 0;
            foreach (KeyValuePair<Type, IEnumerable<object>> set in _cachedSets.ToList())
            {
                foreach (object row in set.Value)
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
                        else if (prop.PropertyType.IsGenericType && typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) &&
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
            var checkSet = Set(value.GetType());
            if (checkSet.Contains(value))
            {
                return false;
            }

            MethodInfo methodToCall = checkSet.GetType().GetMethod("Add", new[] { value.GetType() });
            methodToCall.Invoke(checkSet, new[] { value });
            return true;
        }

        public IDbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            Type entityType = typeof(TEntity);
            return (IDbSet<TEntity>)(Set(entityType));
        }

        private IEnumerable<object> Set(Type entityType)
        {
            if (!_cachedSets.ContainsKey(entityType))
            {
                _cachedSets[entityType] = (IEnumerable<object>)Activator.CreateInstance(typeof(MockedDbSet<>).MakeGenericType(entityType), new[] { this });
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
            return type.IsClass && !string.IsNullOrEmpty(type.Namespace) && type.Namespace.StartsWith("Data.Entities");
        }

        public PropertyInfo EntityPrimaryKeyPropertyInfo(object entity)
        {
            var entityType = entity.GetType();
            List<PropertyInfo> keys = entityType.GetProperties().Where(p => p.GetCustomAttributes(typeof(KeyAttribute), true).Any()).ToList();
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
