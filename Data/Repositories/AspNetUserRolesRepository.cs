using System;
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

        public void AssignRoleIDToUser(string roleID, string userID)
        {
            var existingRoleInDB = GetQuery().Any(ur => ur.RoleId == roleID && ur.UserId == userID);
            if (!DBSet.Local.Any(ur => ur.RoleId == roleID && ur.UserId == userID) && !existingRoleInDB)
                Add(new AspNetUserRolesDO { RoleId = roleID, UserId = userID });
        }

        public void AssignRoleToUser(string roleName, string userID)
        {
            string roleID = GetRoleID(roleName);
            AssignRoleIDToUser(roleID, userID);
        }

        public void RevokeRoleIDFromUser(string roleID, string userID)
        {
            var existingRoles = GetQuery().Where(ur => ur.RoleId == roleID && ur.UserId == userID).ToList();
            foreach (var existinguserRole in existingRoles)
                Remove(existinguserRole);
        }

        public void RevokeRoleFromUser(string roleName, string userID)
        {
            string roleID = GetRoleID(roleName);
            RevokeRoleIDFromUser(roleID, userID);
        }

        public bool UserHasRoleID(string roleID, string userID)
        {
            return GetQuery().Any(ur => ur.RoleId == roleID && ur.UserId == userID);
        }

        public bool UserHasRole(string roleName, string userID)
        {
            string roleID = GetRoleID(roleName);
            return UserHasRoleID(roleID, userID);
        }

        private String GetRoleID(String roleName)
        {
            var roleID = UnitOfWork.AspNetRolesRepository.DBSet.Local.Where(r => r.Name == roleName).Select(r => r.Id).FirstOrDefault();
            if (roleID == null)
                roleID = UnitOfWork.AspNetRolesRepository.GetQuery().Where(r => r.Name == roleName).Select(r => r.Id).FirstOrDefault();
            return roleID;
        }
    }

    public interface IAspNetUserRolesRepository : IGenericRepository<AspNetUserRolesDO>
    {
        void AssignRoleToUser(string roleName, string userID);
        void RevokeRoleFromUser(string roleID, string userID);
        bool UserHasRole(string roleID, string userID); 
    }
}
