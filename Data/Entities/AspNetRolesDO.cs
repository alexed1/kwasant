using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Interfaces;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Data.Entities
{
    [Table("AspNetRoles")]
    public class AspNetRolesDO : IdentityRole,  IAspNetRoles
    {
        //[Key]
        //public string Id { get; set; }
        //public string Name { get; set; }
        //public virtual ICollection<UserDO> Users { get; set; }
    }
}
