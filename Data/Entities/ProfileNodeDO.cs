using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class ProfileNodeDO
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Profile"), Required]
        public int? ProfileID { get; set; }
        public virtual ProfileDO Profile { get; set; }

        [ForeignKey("ParentNode")]
        public int? ParentNodeID { get; set; }
        public ProfileNodeDO ParentNode { get; set; }

        [InverseProperty("ParentNode")]
        public virtual IList<ProfileNodeDO> ChildNodes { get; set; }

        public string Name { get; set; }

        [InverseProperty("ProfileNode")]
        public virtual IList<ProfileItemDO> ProfileItems { get; set; } 
    }
}
