using System;
using System.Collections.Generic;
using System.Linq;
using Data.Interfaces;
using KwasantCore.Services;
using StructureMap;

namespace KwasantWeb.ViewModels
{
    public class UsersAdminVM
    {
        public UsersAdminVM()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Roles = uow.AspNetRolesRepository.GetAll().Select(r => new RoleData { Id = r.Id, Name = r.Name}).ToList();
            }
        }        

        public String UserId { get; set; }
        public String FirstName { get; set; }
        public String PreviousFirstName { get; set; }
        public String LastName { get; set; }
        public String PreviousLasttName { get; set; }
        public int EmailAddressID { get; set; }
        public String EmailAddress { get; set; }
        public String PreviousEmailAddress { get; set; }
        public String RoleId { get; set; }
        public String PreviousRoleId { get; set; }
        public String Role { get; set; }
        public List<RoleData> Roles { get; set; }
        
    }
}
