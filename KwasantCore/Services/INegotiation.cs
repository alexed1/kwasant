using System;
using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;

namespace KwasantCore.Services
{
    public interface INegotiation
    {
        List<Int32> GetAnswerIDs(NegotiationDO curNegotiationDO);
        IList<Int32?> GetAnswerIDsByUser(NegotiationDO curNegotiationDO, UserDO curUserDO, IUnitOfWork uow);
    }
}