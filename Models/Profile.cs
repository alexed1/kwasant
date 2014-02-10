using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Shnexy.Models
{

    public class Profile
    {

        public int Id { get; set; }
        public int UserId { get; set; }
        public ICollection<Registration> Registrations { get; set; }
        public Profile()
        {
            Registrations = new List<Registration> { };
        }
    }

   
}