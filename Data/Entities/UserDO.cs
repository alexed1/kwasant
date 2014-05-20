using System.ComponentModel.DataAnnotations;
using Data.Interfaces;

namespace Data.Entities
{
    public class UserDO : IUser
    {
        [Key]
        public int Id { get; set; }
    }
}
