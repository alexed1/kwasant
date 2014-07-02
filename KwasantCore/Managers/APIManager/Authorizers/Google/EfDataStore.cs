using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Interfaces;
using Google.Apis.Util.Store;
using Newtonsoft.Json;
using StructureMap;

namespace KwasantCore.Managers.APIManager.Authorizers.Google
{
    /// <summary>
    /// Entity Framework implementation for storing Google tokens in the database.
    /// </summary>
    /// <remarks>
    /// EfDataStore stores Google tokens at user's GoogleAuthData field in JSON format. 
    /// As the data is generic the approach is to serialize a value to JSON first and then serialize entire pair.
    /// In the same way at the first step of value retrieval GoogleAuthData string is deserialized into a string dictionary and at the second step string value under needed key deserialized into needed type instance.
    /// </remarks>
    class EfDataStore : IDataStore
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