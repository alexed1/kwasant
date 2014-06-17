using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Interfaces;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Data.Entities
{

    [Table("AspNetUserRoles")]
    public class AspNetUserRolesDO : IdentityUserRole, IAspNetUserRoles
    {
        //public String UserId { get; set; }
        //public String FirstName { get; set; }
        //public String LasttName { get; set; }
        //public String RoleId { get; set; }
        //public String Role { get; set; }
        //public String Role { get; set; }       

        //public virtual ICollection<UserDO> Users { get; set; }
        public virtual ICollection<AspNetRolesDO> Roles { get; set; }  

    }
}
