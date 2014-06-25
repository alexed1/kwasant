using System;
using System.ComponentModel.DataAnnotations;

namespace Data.Entities
{
    public class BookingRequestStatusDO
    {
        [Key]
        public int Id { get; set; }
        public String Name { get; set; }
    }
}
