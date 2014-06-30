using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Interfaces;
using Google.Apis.Util.Store;
using Newtonsoft.Json;
using StructureMap;

namespace KwasantWeb.Controllers.GoogleCalendar
{
    public class EfDataStore : IDataStore
    {
        private readonly string _userId;

        public EfDataStore(string userId)
        {
            _userId = userId;
        }

        private Dictionary<string, string> GetStore(string authData)
        {
            return string.IsNullOrEmpty(authData)
                       ? new Dictionary<string, string>()
                       : JsonConvert.DeserializeObject<Dictionary<string, string>>(authData);
        }

        #region Implementation of IDataStore

        public async Task StoreAsync<T>(string key, T value)
        {
            System.Diagnostics.Debug.WriteLine("EF: Store: key={0}, oftype={1}, value={2}", key, typeof(T), value);
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curUserDO = uow.UserRepository.GetByKey(_userId);
                var store = GetStore(curUserDO.GoogleAuthData);
                store[key] = JsonConvert.SerializeObject(value);
                curUserDO.GoogleAuthData = JsonConvert.SerializeObject(store);
                uow.SaveChanges();
            }
        }

        public async Task DeleteAsync<T>(string key)
        {
            System.Diagnostics.Debug.WriteLine("EF: Delete: key={0}, oftype={1}", key, typeof(T));
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curUserDO = uow.UserRepository.GetByKey(_userId);
                var store = GetStore(curUserDO.GoogleAuthData);
                store.Remove(key);
                curUserDO.GoogleAuthData = JsonConvert.SerializeObject(store);
                uow.SaveChanges();
            }
        }

        public async Task<T> GetAsync<T>(string key)
        {
            System.Diagnostics.Debug.WriteLine("EF: Get: key={0}, oftype={1}", key, typeof(T));
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curUserDO = uow.UserRepository.GetByKey(_userId);
                var store = GetStore(curUserDO.GoogleAuthData);
                string value;
                return store.TryGetValue(key, out value)
                           ? JsonConvert.DeserializeObject<T>(value)
                           : default(T);
            }
        }

        public async Task ClearAsync()
        {
            System.Diagnostics.Debug.WriteLine("EF: Clear");
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curUserDO = uow.UserRepository.GetByKey(_userId);
                var store = GetStore(curUserDO.GoogleAuthData);
                store.Clear();
                curUserDO.GoogleAuthData = JsonConvert.SerializeObject(store);
                uow.SaveChanges();
            }
        }

        #endregion
    }
}