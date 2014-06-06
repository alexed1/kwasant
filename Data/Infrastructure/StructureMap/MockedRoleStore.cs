using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Data.Infrastructure.StructureMap
{
    public class MockedRoleStore : IKwasantRoleStore
    {
        private IUnitOfWork _uow;
        public void Dispose()
        {
            _uow.Dispose();
        }

        public Task CreateAsync(IdentityRole role)
        {
            _uow.AspNetRolesRepository.Add(role);
            return Task.FromResult<object>(null);
        }

        public Task UpdateAsync(IdentityRole role)
        {
            return Task.FromResult<object>(null);
        }

        public Task DeleteAsync(IdentityRole role)
        {
            _uow.AspNetRolesRepository.Remove(role);
            return Task.FromResult<object>(null);
        }

        public Task<IdentityRole> FindByIdAsync(string roleId)
        {
            var role = _uow.AspNetRolesRepository.GetQuery().FirstOrDefault(r => r.Id == roleId);
            return Task.FromResult(role);
        }

        public Task<IdentityRole> FindByNameAsync(string roleName)
        {
            var role = _uow.AspNetRolesRepository.GetQuery().FirstOrDefault(r => r.Name == roleName);
            return Task.FromResult(role);
        }

        public IRoleStore<IdentityRole, string> SetUnitOfWork(IUnitOfWork uow)
        {
            _uow = uow;
            return this;
        }
    }
}
