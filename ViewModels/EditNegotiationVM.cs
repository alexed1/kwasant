using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Data.States.Templates;

namespace KwasantWeb.ViewModels
{
    public class EditNegotiationVM
    {
        public int Id { get; set; }        
        public int RequestId { get; set; }
        public int State { get; set; }
        public string Name { get; set; }
        public List<QuestionViewModel> Questions { get; set; }

        public string AttendeeList { get; set; }
    }
}