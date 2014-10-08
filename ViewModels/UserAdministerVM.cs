using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Data.Entities;

namespace KwasantWeb.ViewModels
{
    public class UserAdministerVM
    {
        public UserDO User { get; set; }
        public String Role { get; set; }
    }
}