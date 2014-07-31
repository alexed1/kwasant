using System.ComponentModel.DataAnnotations;

namespace Data.Entities.Constants
{
    public class ServiceAuthorizationTypeRow
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