using System.Collections.Generic;
using Data.Entities;
using Data.States.Templates;

namespace Data.Interfaces
{
    public interface IClarificationRequest : IEmail
    {

        int ClarificationRequestState { get; set; }
        _ClarificationRequestStateTemplate ClarificationRequestStateTemplate { get; set; }

    }
}
