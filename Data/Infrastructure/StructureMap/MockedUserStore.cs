using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Microsoft.AspNet.Identity;

namespace Data.Infrastructure.StructureMap
{
    public class MockedUserStore : IKwasantUserStore
    {
        private IUnitOfWork _uow;
        public IUserStore<UserDO> SetUnitOfWork(IUnitOfWork uow)
        {
            _uow = uow;
            return this;
        }

        public void Dispose()
        {
            _uow.Dispose();
        }

        public Task CreateAsync(UserDO user)
        {
            _uow.UserRepository.Add(user);
            return Task.FromResult<object>(null);
        }

        public Task UpdateAsync(UserDO user)
        {
            return Task.FromResult<object>(null);
        }

        public Task DeleteAsync(UserDO user)
        {
            _uow.UserRepository.Remove(user);
            return Task.FromResult<object>(null);
        }

        public Task<UserDO> FindByIdAsync(string userId)
        {
            var userDO = _uow.UserRepository.GetQuery().FirstOrDefault(r => r.Id == userId);
            return Task.FromResult(userDO);
        }

        public Task<UserDO> FindByNameAsync(string userName)
        {
            var userDO = _uow.UserRepository.GetQuery().FirstOrDefault(r => r.UserName == userName);
            return Task.FromResult(userDO);
        }
    }
}
