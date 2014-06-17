using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Data.Interfaces;

namespace KwasantWeb.ViewModels
{
    public class ClarificationRequestViewModel
    {
        public ClarificationRequestViewModel()
        {
            
        }
/*

        public ClarificationRequestViewModel(IClarificationRequest clarificationRequest)
        {
            BookingRequestId = clarificationRequest.BookingRequestId;
            Questions = clarificationRequest.Questions.Select(q => new ClarificationQuestionViewModel(q)).ToArray();
        }
*/

        public int Id { get; set; }
        public int BookingRequestId { get; set; }
        public string Recipients { get; set; }
        public string Question { get; set; }
    }
}