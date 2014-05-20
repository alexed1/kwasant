using System.ComponentModel.DataAnnotations;

namespace Data.Interfaces
{
    public interface IUser
    {
        [Key]
        int Id { get; set; }
    }
}