using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Constants;

namespace Data.Entities.Constants
{
    public class ServiceAuthorizationTypeRow : IConstantRow<ServiceAuthorizationType>
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [MaxLength(16)]
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}