using System;
using System.ComponentModel.DataAnnotations;
using Data.Entities.Enumerations;

namespace Data.Entities
{
    public class CommunicationConfigurationDO
    {
        [Key]
        public int Id { get; set; }
        public CommunicationType Type { get; set; }
        public String ToAddress { get; set; }
    }
}
