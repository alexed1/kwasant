using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Mime;
using Data.Interfaces;

namespace Data.Entities
{
    public class AttachmentDO : StoredFileDO, IAttachment
    {
        [ForeignKey("Email")]
        public int EmailID { get; set; }
        [Required]
        public EmailDO Email { get; set; }
        public String Type { get; set; }

        public String ContentID { get; set; }

        public bool BoundaryEmbedded { get; set; }
    }
}
