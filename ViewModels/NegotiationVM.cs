﻿using System;
using System.Collections.Generic;

namespace KwasantWeb.ViewModels
{
    public class NegotiationVM
    {
        public NegotiationVM()
        {
            Questions = new List<NegotiationQuestionVM>();
            Attendees = new List<string>();
        }

        public int? Id { get; set; }
        public int BookingRequestID { get; set; }
        public int State { get; set; }
        public string Name { get; set; }
        public List<NegotiationQuestionVM> Questions { get; set; }
        public List<String> Attendees { get; set; }
    }

    public class NegotiationQuestionVM
    {
        public NegotiationQuestionVM()
        {
            Answers = new List<NegotiationAnswerVM>();
        }

        public int Id { get; set; }
        public string Text { get; set; }
        public int Status { get; set; }
        public int NegotiationId { get; set; }
        public List<NegotiationAnswerVM> Answers { get; set; }
        public string AnswerType { get; set; }
        public int? CalendarID { get; set; }
        public List<QuestionCalendarEventVM> CalendarEvents { get; set; }
    }

    public class NegotiationAnswerVM
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public int Status { get; set; }
        public string UserId { get; set; }
        public string Text { get; set; }
    }
}