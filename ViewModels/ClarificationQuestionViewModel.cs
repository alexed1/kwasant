using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Data.Entities.Enumerations;
using Data.Interfaces;

namespace KwasantWeb.ViewModels
{
    public class ClarificationQuestionViewModel
    {
/*
        public ClarificationQuestionViewModel(IClarificationQuestion question)
        {
            Text = question.Text;
            Status = question.Status;
        }
*/

        public int Id { get; set; }
        public string Text { get; set; }
        public ClarificationQuestionStatus Status { get; set; }
    }
}