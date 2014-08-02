using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities.Constants;
using Data.Interfaces;

namespace Data.Entities
{
    /// <summary>
    /// Presents a remote calendar provider such as Google, Yahoo, etc.
    /// </summary>
    public class RemoteCalendarProviderDO : IRemoteCalendarProvider
    {
        [Key]
        public int Id { get; set; }
        [Index(IsUnique = true), MaxLength(32)]
        public string Name { get; set; }

        [ForeignKey("AuthType"), Required]
        public int AuthTypeID { get; set; }
        public virtual ServiceAuthorizationTypeRow AuthType { get; set; }
        
        /// <summary>
        /// JSON string for storing Kwasant application credentials for operating with provider (for instance: ClientId, ClientSecret and Scopes for OAuth authorization)
        /// </summary>
        public string AppCreds { get; set; }

        /// <summary>
        /// Base url for accessing CalDAV API.
        /// </summary>
        public string CalDAVEndPoint { get; set; }
    }
}
