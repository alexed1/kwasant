using System;
using System.ComponentModel.DataAnnotations;
namespace Data.Entities
{
   public class IncidentDO
    {
        [Key]
        public int Id { get; set; }
        public DateTimeOffset CreateTime { get; set; }
        public String PrimaryCategory { get; set; }
        public String SecondaryCategory { get; set; }
        public String Activity { get; set; }
        public int Priority { get; set; }
        public string Notes { get; set; }
    }
}
