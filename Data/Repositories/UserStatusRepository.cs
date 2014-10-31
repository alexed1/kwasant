using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces;
using Data.States.Templates;

namespace Data.Repositories
{
    public class UserStatusRepository : GenericRepository<_UserStateTemplate>, IUserDOStatusRepository
    {
        public UserStatusRepository(IUnitOfWork uow)
            : base(uow)
        {
            
        }
    }
    public interface IUserDOStatusRepository : IGenericRepository<_UserStateTemplate>
    {

    }
}
