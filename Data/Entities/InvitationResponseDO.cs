using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces;
using Data.States.Templates;

namespace Data.Entities
{
    public class InvitationResponseDO : EmailDO, IInvitationResponse
    {
        [ForeignKey("Attendee")]
        public int AttendeeId { get; set; }
        public virtual AttendeeDO Attendee { get; set; }
    }
}
