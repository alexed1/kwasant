using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KwasantCore.Services;
using Data.Infrastructure;
using Data.Repositories;
using Data.Interfaces;
using StructureMap;

namespace ViewModel.Models
{
    public class UsersAdminViewModel
    {
        IUnitOfWork _uow;
        public UsersAdminViewModel()
        {
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();
            Role role = new Role(_uow);
            List<RoleData> roleList = role.GetRoles();
            Roles = roleList;
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
