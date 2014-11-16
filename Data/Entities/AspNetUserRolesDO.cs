using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Interfaces;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Data.Entities
{

    [Table("AspNetUserRoles")]
    public class AspNetUserRolesDO : IdentityUserRole, IAspNetUserRolesDO, IBaseDO, ICreateHook
    {
        public virtual ICollection<AspNetRolesDO> Roles { get; set; }

        public DateTimeOffset LastUpdated { get; set; }
        public DateTimeOffset CreateDate { get; set; }

        void ICreateHook.BeforeCreate()
        {
            CreateDate = DateTimeOffset.Now;
        }

        void ICreateHook.AfterCreate()
        {
        }
    }
}
