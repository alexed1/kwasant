using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Shnexy.Models
{
    public class Message
    {
        [Key]
        int Id;
        string Body;
        ICollection<int> RecipientList;
        int SenderId;


    }
}