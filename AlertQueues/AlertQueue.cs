using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace KwasantWeb.AlertQueues
{
    public interface IPersonalAlertQueue
    {
        IEnumerable<object> GetUpdates();
    }

    public interface IPersonalAlertQueue<out T> : IPersonalAlertQueue
    {
        IEnumerable<T> GetUpdates(Func<T, bool> predicate);
    }

    public class PersonalAlertQueue<T> : IPersonalAlertQueue<T>
        where T : class
    {
        private readonly SynchronizedCollection<T> _baseCollection = new SynchronizedCollection<T>();
        public IEnumerable<object> GetUpdates()
        {
            return GetUpdates(null);
        }
        public IEnumerable<T> GetUpdates(Func<T, bool> predicate)
        {
            if (predicate == null)
            {
                var itemsToReturn = new List<T>(_baseCollection);
                _baseCollection.Clear();
                return itemsToReturn;
            }
            else
            {
                var itemsToReturn = new List<T>(_baseCollection.Where(predicate));
                foreach (var itemToReturn in itemsToReturn)
                    _baseCollection.Remove(itemToReturn);

                return itemsToReturn;
            }
        }

        protected void AppendUpdate(T update)
        {
            _baseCollection.Add(update);
        }
    }

    public interface IStaticQueue
    {
        void PruneOldEntries();
    }
    public interface ISharedAlertQueue
    {
        IEnumerable<object> GetUpdates(String guid);
    }

    public interface ISharedAlertQueue<out T> : ISharedAlertQueue
    {
        void RegisterInterest(string guid);
        IEnumerable<T> GetUpdates(String guid, Func<T, bool> predicate);
    }
    public class SharedSharedAlertQueue<T> : ISharedAlertQueue<T>, IStaticQueue
        where T : class
    {
        private readonly TimeSpan _expireInterestedPartiesAfter = TimeSpan.FromMinutes(15);
        private readonly ConcurrentDictionary<Object, DateTime> _objectExpirations = new ConcurrentDictionary<object, DateTime>();
        private readonly SynchronizedCollection<T> _baseCollection = new SynchronizedCollection<T>(); 
        private readonly ConcurrentDictionary<String, SynchronizedCollection<T>> _interestedPartyQueues = new ConcurrentDictionary<String, SynchronizedCollection<T>>();
        
        public void RegisterInterest(string guid)
        {
            var newList = new SynchronizedCollection<T>();
            _interestedPartyQueues[guid] = newList;

            MarkExpiration(newList, _expireInterestedPartiesAfter);
        }

        public IEnumerable<object> GetUpdates(string guid)
        {
            return GetUpdates(guid, null);
        }

        public virtual IEnumerable<T> GetUpdates(String guid, Func<T, bool> predicate)
        {
            if (_interestedPartyQueues.ContainsKey(guid))
            {
                var interestedPartyQueue = _interestedPartyQueues[guid];

                MarkExpiration(interestedPartyQueue, _expireInterestedPartiesAfter);

                var updates = predicate != null
                    ? interestedPartyQueue.Where(predicate).ToList()
                    : interestedPartyQueue.ToList();

                MarkUpdatesRead(updates);   // This may be changed to have an explict 'read' call (if the user clicks the notification)
                                            // For now, though - we assume they read it when it's displayed

                if (predicate == null)
                    interestedPartyQueue.Clear();
                else
                {
                    foreach (var update in updates)
                        interestedPartyQueue.Remove(update);
                }
                return updates;
            }

            return new T[0];
        }

        protected void AppendUpdate(T update)
        {
            if (_interestedPartyQueues.Any())
            {
                MarkExpiration(update, _expireInterestedPartiesAfter);

                foreach (var interestedPartyQueue in _interestedPartyQueues)
                {
                    _interestedPartyQueues[interestedPartyQueue.Key].Add(update);
                }
                _baseCollection.Add(update);
            }
            else
            {
                ObjectExpired(update);
            }
        }


        private void MarkUpdatesRead(IEnumerable<T> updates)
        {
            MarkUpdatesRead(updates.ToArray());
        }

        private void MarkUpdatesRead(params T[] updates)
        {
            foreach (var update in updates)
                _baseCollection.Remove(update);
        }

        private void MarkExpiration(Object obj, TimeSpan timeSpan)
        {
            _objectExpirations[obj] = DateTime.Now.Add(timeSpan);
        }

        private bool ObjectHasExpired(Object obj)
        {
            if (_objectExpirations.ContainsKey(obj))
                return _objectExpirations[obj] < DateTime.Now;

            return false;
        }

        protected virtual void ObjectExpired(T obj)
        {
            //Do nothing - overridable
        }

        public void PruneOldEntries()
        {
            //Prune listeners
            foreach (var interestedPartyQueue in _interestedPartyQueues)
            {
                if (ObjectHasExpired(interestedPartyQueue.Value))
                {
                    SynchronizedCollection<T> garbage;
                    const int maxAttempts = 5;
                    var currAttempt = 1;
                    bool success;
                    do
                    {
                        success = _interestedPartyQueues.TryRemove(interestedPartyQueue.Key, out garbage);
                    } while (!success && currAttempt++ < maxAttempts);
                }
            }

            //Remove expired updates and dispatch the call on them
            var clonedList = new List<T>(_baseCollection);
            foreach (var update in clonedList)
            {
                if (ObjectHasExpired(update))
                {
                    ObjectExpired(update);
                    _baseCollection.Remove(update);
                }
            }
        }
    }

    public interface IUserUpdateData
    {
        String UserID { get; set; }
    }
}