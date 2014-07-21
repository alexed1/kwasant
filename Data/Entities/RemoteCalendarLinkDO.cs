using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Interfaces;

namespace Data.Entities
{
    public class RemoteCalendarLinkDO : IRemoteCalendarLink
    {
        [Key]
        public int Id { get; set; }
        
        public string RemoteCalendarName { get; set; }
        
        [Required, ForeignKey("LocalCalendar")]
        public int LocalCalendarID { get; set; }
        public virtual CalendarDO LocalCalendar { get; set; }
        
        [Required, ForeignKey("Provider")]
        public int ProviderID { get; set; }
        public virtual RemoteCalendarProviderDO Provider { get; set; }

        public DateTimeOffset DateSynchronizationAttempted { get; set; }
        public DateTimeOffset DateSynchronized { get; set; }
        public string LastSynchronizationResult { get; set; }

        [NotMapped]
        ICalendar IRemoteCalendarLink.LocalCalendar
        {
            get { return LocalCalendar; }
            set { LocalCalendar = (CalendarDO)value; }
        }

        [NotMapped]
        IRemoteCalendarProvider IRemoteCalendarLink.Provider
        {
            get { return Provider; }
            set { Provider = (RemoteCalendarProviderDO) value; }
        }
    }
}