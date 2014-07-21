using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces;

namespace Data.Entities
{
    public class RemoteCalendarProviderDO : IRemoteCalendarProvider
    {
        [Key]
        public int Id { get; set; }
        [Index(IsUnique = true), MaxLength(32)]
        public string Name { get; set; }

        [ForeignKey("AuthType"), Required]
        public int AuthTypeID { get; set; }
        public virtual ServiceAuthorizationTypeDO AuthType { get; set; }
        
        public string AppCreds { get; set; }

        public string CalDAVEndPoint { get; set; }
    }
}
