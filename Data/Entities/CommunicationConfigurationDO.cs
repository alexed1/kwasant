using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Entities.Constants;

namespace Data.Entities
{
    public class CommunicationConfigurationDO
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("CommunicationType")]
        public int CommunicationTypeID { get; set; }
        public CommunicationTypeRow CommunicationType { get; set; }
        
        public String ToAddress { get; set; }
    }
}
