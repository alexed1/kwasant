using System;
using System.ComponentModel.DataAnnotations;
using Data.DataAccessLayer.Interfaces;

namespace Data.Models
{
    public class AttachmentDO : StoredFileDO, IAttachment
    {
        [Key]
        public int AttachmentID { get; set; }

        public int EmailID { get; set; }
        [Required]
        public EmailDO EmailDO { get; set; }
        public String Type { get; set; }
    }
}
