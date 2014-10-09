using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Data.Entities;
using Data.Interfaces;
using KwasantCore.Services;
using StructureMap;

namespace KwasantWeb.ViewModels
{
    public class UserCreateVM
    {
        public UserDO User { get; set; }
        public string UserRole { get; set; }
    }
}