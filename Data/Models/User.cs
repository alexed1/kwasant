using System.ComponentModel.DataAnnotations;
using Data.DataAccessLayer.Interfaces;

namespace Data.Models
{
    public class User : IUser
    {
        [Key]
        public int UserID { get; set; }
    }
}
