using System.ComponentModel.DataAnnotations;

namespace Data.Entities.Enumerations
{
    public class ServiceAuthorizationType
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