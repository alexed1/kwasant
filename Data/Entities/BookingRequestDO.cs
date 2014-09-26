using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Infrastructure;
using Data.Interfaces;
using Data.States.Templates;

namespace Data.Entities
{
    public class BookingRequestDO : EmailDO, ICreateHook
    {
        public BookingRequestDO()
        {
            Calendars = new List<CalendarDO>();
            Negotiations = new List<NegotiationDO>();
        }

        [Required, ForeignKey("User")]
        public string UserID { get; set; }        
        public virtual UserDO User { get; set; }

        public virtual List<InstructionDO> Instructions { get; set; }

        [InverseProperty("BookingRequest")]
        public virtual IList<NegotiationDO> Negotiations { get; set; }

        [InverseProperty("BookingRequests")]
        public virtual List<CalendarDO> Calendars { get; set; }

        [Required, ForeignKey("BookingRequestStateTemplate")]
        public int State { get; set; }
        public virtual _BookingRequestStateTemplate BookingRequestStateTemplate { get; set; }

        public string BookerID { get; set; }
        
        public void AfterCreate()
        {
            AlertManager.BookingRequestCreated(Id);
        }
    }
}
