using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Shnexy.Models
{

    public class Profile
    {
        int Id;
        ICollection<Registration> Registrations;

    }
}