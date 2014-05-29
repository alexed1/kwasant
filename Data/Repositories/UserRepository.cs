using System.Linq;
using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class UserRepository : GenericRepository<UserDO>, IUserRepository
    {
        internal UserRepository(IDBContext dbContext)
            : base(dbContext)
        {
            
        }

        public UserDO GetOrCreateUser(BookingRequestDO currMessage)
        {
            string fromEmailAddress = currMessage.From.Address;
            UserRepository userRepo = UnitOfWork.UserRepository;
            UserDO curUser = userRepo.DBSet.Local.FirstOrDefault(c => c.EmailAddress.Address == fromEmailAddress);
            if(curUser == null)
                curUser = userRepo.GetQuery().FirstOrDefault(c => c.EmailAddress.Address == fromEmailAddress);

            if (curUser == null)
            {
                curUser = new UserDO
                {
                    UserName = fromEmailAddress,
                    EmailAddress = UnitOfWork.EmailAddressRepository.GetOrCreateEmailAddress(fromEmailAddress, currMessage.From.Name),
                    FirstName = currMessage.From.Name
                };
                userRepo.Add(curUser);
            }
            return curUser;
        }
    }
}