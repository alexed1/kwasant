using System.ComponentModel.DataAnnotations;

namespace Data.Interfaces
{
    public interface IAttachment
    {
        [Key]
        int Id { get; set; }
    }
}