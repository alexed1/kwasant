using System.Linq;
using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class AspNetUserRolesRepository : GenericRepository<AspNetUserRolesDO>, IAspNetUserRolesRepository
    {

        internal AspNetUserRolesRepository(IUnitOfWork uow)
            : base(uow)
        {
            
        }

        public void AssignRoleToUser(string roleID, string userID)
        {
            if (!GetQuery().Any(ur => ur.RoleId == roleID && ur.UserId == userID))
                Add(new AspNetUserRolesDO {RoleId = roleID, UserId = userID});
        }

        public void RevokeRoleFromUser(string roleID, string userID)
        {
            var existingRoles = GetQuery().Where(ur => ur.RoleId == roleID && ur.UserId == userID).ToList();
            foreach (var existinguserRole in existingRoles)
                Remove(existinguserRole);
        }

        public bool UserHasRole(string roleID, string userID)
        {
            return GetQuery().Any(ur => ur.RoleId == roleID && ur.UserId == userID);
        }
    }

    public interface IAspNetUserRolesRepository : IGenericRepository<AspNetUserRolesDO>
    {
        void AssignRoleToUser(string roleID, string userID);
        void RevokeRoleFromUser(string roleID, string userID);
        bool UserHasRole(string roleID, string userID); 
    }
}
