using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class ServiceAuthorizationTypeDO
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(16)]
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}