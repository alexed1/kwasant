using System;
using System.ComponentModel.DataAnnotations;
using Data.DataAccessLayer.Interfaces;

namespace Data.Models
{
    public class Attachment : StoredFile, IAttachment
    {
        [Key]
        public int AttachmentID { get; set; }

        public int EmailID { get; set; }
        [Required]
        public Email Email { get; set; }
        public String Type { get; set; }
    }
}
