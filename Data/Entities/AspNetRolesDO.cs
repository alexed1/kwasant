using System;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Interfaces;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Data.Entities
{
    [Table("AspNetRoles")]
    public class AspNetRolesDO : IdentityRole, IAspNetRolesDO, IBaseDO
    {
        public DateTimeOffset LastUpdated { get; set; }
    }
}
