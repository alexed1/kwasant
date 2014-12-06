using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces;

namespace Data.Entities
{
    public class HistoryItemDO : BaseDO, IHistoryItemDO
    {
        [Key]
        public int Id { get; set; }
        public string ObjectId { get; set; }
        public string BookerId { get; set; }
        public string CustomerId { get; set; }
        public string PrimaryCategory { get; set; }
        public string SecondaryCategory { get; set; }
        public string Activity { get; set; }
        public string Data { get; set; }
        public string Status { get; set; }
    }
}
