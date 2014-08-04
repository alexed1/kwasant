using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Data.Entities.Constants;

namespace KwasantWeb.ViewModels
{   
    public class AnswerViewModel
    {
        public int Id { get; set; }        
        public int QuestionID { get; set; }       
        public int AnswerStatusID { get; set; }
        public AnswerStatusRow  Status { get; set; }
        public string ObjectsType { get; set; }
        public string Text { get; set; }
    }
}