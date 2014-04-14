using System;
using System.ComponentModel.DataAnnotations;

namespace Data.Models
{
    public class InstructionDO
    {
        [Key]
        public int InstructionID { get; set; }
        public string Name { get; set; }
        public String Category { get; set; }
    }
}
