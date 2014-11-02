using System;
using System.Collections.Generic;
using System.Linq;

namespace KwasantWeb.AlertQueues
{
    public interface IAlertQueue
    {
        IEnumerable<object> GetItems();
        int FilterObjectID { get; set; }
    }
    
    public interface IAlertQueue<out T> : IAlertQueue
    {
        new IEnumerable<T> GetItems();
        IEnumerable<T> GetItems(Func<T, bool> predicate);
    }

    public interface IPrunableAlertQueue
    {
        void PruneExpired();
    }

    public abstract class BaseAlertQueue<T> : IAlertQueue<T>
        where T : class
    {
        protected readonly SynchronizedCollection<T> InnerList = new SynchronizedCollection<T>();

        public int FilterObjectID { get; set; }

        public virtual IEnumerable<T> GetItems(Func<T, bool> predicate)
        {
            return InnerList.Where(predicate);
        }
        public virtual IEnumerable<T> GetItems()
        {
            return InnerList;
        }

        IEnumerable<object> IAlertQueue.GetItems()
        {
            return GetItems();
        }
    }

    public abstract class ExpiringAlertQueue<T> : BaseAlertQueue<T>, IPrunableAlertQueue
        where T : class
    {
        public abstract void ExpireItem(T item);

        private readonly TimeSpan _expireAfter;        
        private readonly bool _removeOnRetrieve;
        
        private readonly Dictionary<T, DateTime> _itemExpirations = new Dictionary<T, DateTime>();

        protected ExpiringAlertQueue(
            TimeSpan expireAfter, 
            bool removeOnRetrieve = true)
        {
            _expireAfter = expireAfter;
            _removeOnRetrieve = removeOnRetrieve;
        }

        protected void AppendItem(T item)
        {
            RemoveExpired();

            InnerList.Add(item);
            _itemExpirations[item] = DateTime.Now.Add(_expireAfter);
        }

        public override IEnumerable<T> GetItems(Func<T, bool> predicate)
        {
            RemoveExpired();

            var items = base.GetItems(predicate).ToList();
            if (_removeOnRetrieve)
                foreach (var item in items)
                    InnerList.Remove(item);

            return items;
        }

        public override IEnumerable<T> GetItems()
        {
            RemoveExpired();

            var items = base.GetItems().ToList();
            if (_removeOnRetrieve)
                InnerList.Clear();
            return items;
        }

        private void RemoveExpired()
        {
            var clonedList = new List<T>(InnerList);
            foreach (var item in clonedList)
            {
                if (DateTime.Now > _itemExpirations[item])
                {
                    InnerList.Remove(item);
                    ExpireItem(item);
                }
            }
        }

        public void PruneExpired()
        {
            RemoveExpired();
        }
    }

    public interface IUserUpdateData
    {
        String UserID { get; set; }
    }
}