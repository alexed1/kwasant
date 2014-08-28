using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;

namespace KwasantCore.Helper
{
   public class UserData
    {
        public UserDO User { get; set; }
        public string RoleId { get; set; }
        public string Role { get; set; }
    }
}
