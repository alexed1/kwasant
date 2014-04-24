using System.ComponentModel.DataAnnotations;

namespace Data.Interfaces
{
    public interface IAttachment
    {
        [Key]
        int AttachmentID { get; set; }
    }
}