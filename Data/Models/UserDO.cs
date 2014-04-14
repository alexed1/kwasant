using System.ComponentModel.DataAnnotations;
using Data.DataAccessLayer.Interfaces;

namespace Data.Models
{
    public class UserDO : IUser
    {
        [Key]
        public int UserID { get; set; }
    }
}
