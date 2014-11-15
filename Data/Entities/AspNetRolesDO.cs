using System;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Interfaces;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Data.Entities
{
    [Table("AspNetRoles")]
    public class AspNetRolesDO : IdentityRole, IAspNetRolesDO, IBaseDO, ISaveHook
    {
        public DateTimeOffset LastUpdated { get; set; }
        public DateTimeOffset CreateDate { get; set; }

        void ISaveHook.BeforeSave()
        {
            CreateDate = DateTimeOffset.Now;
        }
    }
}
