using System;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Interfaces;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Data.Entities
{
    [Table("AspNetRoles")]
    public class AspNetRolesDO : IdentityRole, IAspNetRolesDO, IBaseDO, ICreateHook
    {
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
