using System.Collections.Generic;
using Data.Entities;
using Data.Entities.Constants;

namespace Data.Interfaces
{
    public interface IClarificationRequest : IEmail
    {

        int ClarificationRequestStateID { get; set; }
        ClarificationRequestStateRow ClarificationRequestState { get; set; }

    }
}
