using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class InstructionDO
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public String Category { get; set; }

        [ForeignKey("BookingRequest")]
        public int? BookingRequestID { get; set; }
        public virtual BookingRequestDO BookingRequest { get; set; }
    }
}
