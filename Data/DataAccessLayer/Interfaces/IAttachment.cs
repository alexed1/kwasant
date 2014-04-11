using System;
using System.ComponentModel.DataAnnotations;

namespace Data.DataAccessLayer.Interfaces
{
    public interface IAttachment
    {
        [Key]
        int AttachmentID { get; set; }
    }
}