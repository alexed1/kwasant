using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.DataAccessLayer.Interfaces;

namespace Data.Models
{
    public class Attachment : StoredFile, IAttachment
    {
        [Key]
        public int AttachmentID { get; set; }

        [ForeignKey("Email")] 
        public int EmailID;
        [Required]
        public Email Email { get; set; }
        public String Type { get; set; }
    }
}
