using Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;

namespace Data.Repositories
{
    public class QuestionsRepository : GenericRepository<QuestionDO>, IQuestionsRepository
    {
        internal QuestionsRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }

     public interface IQuestionsRepository : IGenericRepository<QuestionDO>
    {

    }
}
