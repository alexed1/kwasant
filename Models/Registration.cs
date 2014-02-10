using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Shnexy.Models
{
    public class Registration
    {
        public int Id { get; set; }
        public int ServiceId;
        public string Name;
        public string Password;



        public Registration(int serviceId, string name, string password)
        {

            ServiceId = serviceId;
            Name = name;
            Password = password;

        }






    }
}