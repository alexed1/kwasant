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
    public class NegotiationQuestionViewModel
    {
        public int Id { get; set; }
        public int? RequestId { get; set; }
        public QuestionStatus Status { get; set; }
        public string Text { get; set; }
        public string Response { get; set; }
        public List<NegotiationAnswerViewModel> Answers { get; set; }
        //public int NegotiationId { get; set; }
    }
    public class NegotiationAnswerViewModel
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public string Status { get; set; }
        public string ObjectType { get; set; }
        public string UserId { get; set; }
    }
}