using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Data.Entities.Enumerations;
using Data.Interfaces;

namespace KwasantWeb.ViewModels
{
    public class QuestionViewModel
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public QuestionStatus Status { get; set; }
    }
}