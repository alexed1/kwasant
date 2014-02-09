using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Shnexy.Models
{
    public class Message
    {



        public int Id { get; set; }
        public string Body { get; set; }
        public ICollection<int> RecipientList { get; set; }
        public int SenderId { get; set; }


    }
}