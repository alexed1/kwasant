using Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;

namespace Data.Repositories
{
    public class NegotiationsRepository : GenericRepository<NegotiationDO>, INegotiationsRepository
    {
        internal NegotiationsRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }

    public interface INegotiationsRepository : IGenericRepository<NegotiationDO>
    {

    }
}
