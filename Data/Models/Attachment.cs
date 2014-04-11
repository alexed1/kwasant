using System;
using System.ComponentModel.DataAnnotations;
using Data.DataAccessLayer.Interfaces;

namespace Data.Models
{
    public class Attachment : IAttachment
    {
        [Key]
        public int AttachmentID { get; set; }
        public String Name { get; set; }
        public String Type { get; set; }
        public String FileLocation { get; set; }
    }
}
