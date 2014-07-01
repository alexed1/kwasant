using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities.Enumerations;

namespace Data.Interfaces
{
    public interface IQuestion
    {
        int Id { get; set; }
        int? ClarificationRequestId { get; set; }
        QuestionStatus Status { get; set; }
        string Text { get; set; }
        string Response { get; set; }
    }
}
