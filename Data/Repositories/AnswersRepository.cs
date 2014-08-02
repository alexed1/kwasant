using Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;

namespace Data.Repositories
{
    public class AnswersRepository : GenericRepository<AnswerDO>, IAnswersRepository
    {
        internal AnswersRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }

    public interface IAnswersRepository : IGenericRepository<AnswerDO>
    {

    }
}
