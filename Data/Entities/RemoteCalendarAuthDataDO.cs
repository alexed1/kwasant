using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Interfaces;

namespace Data.Entities
{
    public class RemoteCalendarAuthDataDO : IRemoteCalendarAuthData
    {
        [Key]
        public int Id { get; set; }

        [Required, ForeignKey("Provider")]
        public int ProviderID { get; set; }
        
        public virtual RemoteCalendarProviderDO Provider { get; set; }
        
        [Required, ForeignKey("User")]
        public string UserID { get; set; }
        
        public virtual UserDO User { get; set; }
        
        public string AuthData { get; set; }

        [NotMapped]
        IRemoteCalendarProvider IRemoteCalendarAuthData.Provider
        {
            get { return Provider; }
            set { Provider = (RemoteCalendarProviderDO) value; }
        }

        [NotMapped]
        IUser IRemoteCalendarAuthData.User
        {
            get { return User; }
            set { User = (UserDO) value; }
        }
    }
}