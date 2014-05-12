using System.ComponentModel.DataAnnotations;

namespace Data.Interfaces
{
    public interface IUser
    {
        [Key]
        int UserID { get; set; }
    }
}