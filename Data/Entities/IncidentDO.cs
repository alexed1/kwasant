using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
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
        public int ObjectId { get; set; }
        public string CustomerId { get; set; }
        public string BookerId { get; set; }
    }
}
