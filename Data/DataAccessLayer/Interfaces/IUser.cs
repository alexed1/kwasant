using System.ComponentModel.DataAnnotations;

namespace Data.DataAccessLayer.Interfaces
{
    public interface IUser
    {
        [Key]
        int UserID { get; set; }
    }
}