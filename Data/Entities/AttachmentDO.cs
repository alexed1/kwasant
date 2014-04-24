using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.DataAccessLayer.Interfaces;
using Data.Interfaces;

namespace Data.Entities
{
    public class AttachmentDO : StoredFileDO, IAttachment
    {
        [Key]
        public int AttachmentID { get; set; }

        [ForeignKey("Email")]
        public int EmailID { get; set; }
        [Required]
        public EmailDO Email { get; set; }
        public String Type { get; set; }
    }
}
