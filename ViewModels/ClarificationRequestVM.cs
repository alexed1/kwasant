using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Data.Interfaces;

namespace KwasantWeb.ViewModels
{
    public class ClarificationRequestVM
    {
        public int Id { get; set; }
 
        public string Recipients { get; set; }
      
     
    }
}