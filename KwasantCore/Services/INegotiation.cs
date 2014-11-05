using System;
using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;

namespace KwasantCore.Services
{
    public interface INegotiation
    {
        List<Int32> GetAnswers(NegotiationDO curNegotiationDO);
        IList<Int32?> GetAnswersByUser(NegotiationDO curNegotiationDO, UserDO curUserDO, IUnitOfWork uow);
    }
}