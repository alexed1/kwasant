using System;
using System.ComponentModel.DataAnnotations;

namespace Data.DataAccessLayer.Interfaces
{
    public interface IAttachment
    {
        [Key]
        int AttachmentID { get; set; }

        String Name { get; set; }
        String Type { get; set; }
        String FileLocation { get; set; }
    }
}