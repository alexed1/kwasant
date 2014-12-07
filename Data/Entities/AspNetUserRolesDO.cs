using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Interfaces;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Data.Entities
{

    [Table("AspNetUserRoles")]
    public class AspNetUserRolesDO : IdentityUserRole, IAspNetUserRolesDO, ISaveHook
    {
        public virtual ICollection<AspNetRolesDO> Roles { get; set; }

        public DateTimeOffset LastUpdated { get; set; }
        public DateTimeOffset CreateDate { get; set; }

        public void BeforeCreate(IUnitOfWork uow)
        {
            if (CreateDate == default(DateTimeOffset))
                CreateDate = DateTimeOffset.Now;
        }

        public void BeforeSave(IUnitOfWork uow)
        {
            LastUpdated = DateTimeOffset.Now;
        }

    }
}
