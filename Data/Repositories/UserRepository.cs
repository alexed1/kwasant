using System.Linq;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;

namespace Data.Repositories
{
    public class UserRepository : GenericRepository<UserDO>, IUserRepository
    {
        internal UserRepository(IDBContext dbContext)
            : base(dbContext)
        {
            
        }

        public UserDO GetOrCreateUser(BookingRequestDO curMessage)
        {
            string fromEmailAddress = curMessage.From.Address;
            UserRepository userRepo = UnitOfWork.UserRepository;
            UserDO curUser = userRepo.DBSet.Local.FirstOrDefault(c => c.EmailAddress.Address == fromEmailAddress);
            if(curUser == null)
                curUser = userRepo.GetQuery().FirstOrDefault(c => c.EmailAddress.Address == fromEmailAddress);

            if (curUser == null)
            {
                curUser = new UserDO();
                curUser.UserName = fromEmailAddress;
                curUser.FirstName = curMessage.From.Name;
                curUser.EmailAddress = UnitOfWork.EmailAddressRepository.GetOrCreateEmailAddress(fromEmailAddress);
                userRepo.Add(curUser);
                UnitOfWork.SaveChanges();
                AlertManager.CustomerCreated(curUser);
            }
            return curUser;
        }
    }
}