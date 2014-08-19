using System;
using System.Collections.Generic;

namespace KwasantWeb.ViewModels
{
    public class NegotiationViewModel
    {
        public NegotiationViewModel()
        {
            Questions = new List<NegotiationQuestionViewModel>();
            Attendees = new List<string>();
        }

        public int? Id { get; set; }
        public int BookingRequestID { get; set; }
        public int State { get; set; }
        public string Name { get; set; }
        public List<NegotiationQuestionViewModel> Questions { get; set; }
        public List<String> Attendees { get; set; }
    }

    public class NegotiationQuestionViewModel
    {
        public NegotiationQuestionViewModel()
        {
            Answers = new List<NegotiationAnswerViewModel>();
        }

        public int Id { get; set; }
        public string Text { get; set; }
        public int Status { get; set; }
        public int NegotiationId { get; set; }
        public List<NegotiationAnswerViewModel> Answers { get; set; }
        public string AnswerType { get; set; }
        public int? CalendarID { get; set; }
        public List<QuestionCalendarEventViewModel> CalendarEvents { get; set; }
    }

    public class NegotiationAnswerViewModel
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public int Status { get; set; }
        public string UserId { get; set; }
        public string Text { get; set; }
    }
}