using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Microsoft.AspNet.Identity;

namespace Data.Infrastructure.StructureMap
{
    public class MockedRoleStore : IKwasantRoleStore
    {
        private IUnitOfWork _uow;
        public void Dispose()
        {
            _uow.Dispose();
        }

        public Task CreateAsync(AspNetRolesDO role)
        {
            _uow.AspNetRolesRepository.Add(role);
            return Task.FromResult<object>(null);
        }

        public Task UpdateAsync(AspNetRolesDO role)
        {
            return Task.FromResult<object>(null);
        }

        public Task DeleteAsync(AspNetRolesDO role)
        {
            _uow.AspNetRolesRepository.Remove(role);
            return Task.FromResult<object>(null);
        }

        public Task<AspNetRolesDO> FindByIdAsync(string roleId)
        {
            var role = _uow.AspNetRolesRepository.GetQuery().FirstOrDefault(r => r.Id == roleId);
            return Task.FromResult(role);
        }

        public Task<AspNetRolesDO> FindByNameAsync(string roleName)
        {
            var role = _uow.AspNetRolesRepository.GetQuery().FirstOrDefault(r => r.Name == roleName);
            return Task.FromResult(role);
        }

        public IRoleStore<AspNetRolesDO, string> SetUnitOfWork(IUnitOfWork uow)
        {
            _uow = uow;
            return this;
        }
    }
}
