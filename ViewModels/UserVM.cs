using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Data.Entities;

namespace KwasantWeb.ViewModels
{
    public class UserVM
    {
        public UserDO User { get; set; }
        public string RoleId { get; set; }
        public string Role { get; set; }

    }
}