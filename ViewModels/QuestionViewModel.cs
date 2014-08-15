using System;
using System.Collections.Generic;

namespace KwasantWeb.ViewModels
{
    public class QuestionViewModel
    {
        public QuestionViewModel()
        {
            Answers = new List<AnswerViewModel>();
        }

        public int Id { get; set; }
        public string Text { get; set; }
        public int Status { get; set; }
        public int NegotiationId { get; set; }
        public List<AnswerViewModel> Answers { get; set; }
        public string AnswerType { get; set; }
        public int? CalendarID { get; set; }
        public List<QuestionCalendarEventViewModel> CalendarEvents { get; set; }
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