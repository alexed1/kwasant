using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Data.Entities;
using Data.Interfaces;
using KwasantCore.Services;
using StructureMap;

namespace KwasantWeb.ViewModels
{
    public class CreateUserVM
    {
        public CreateUserVM()
        {
            IUnitOfWork _uow = ObjectFactory.GetInstance<IUnitOfWork>();
            Role role = new Role();
            List<RoleData> roleList = role.GetRoles();
            Roles = roleList;
        }
        public List<RoleData> Roles { get; set; }
        public UserDO User { get; set; }
        public string UserRole { get; set; }
    }
}