using System;
using System.Linq;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Validators;

namespace Data.Repositories
{
    public class UserRepository : GenericRepository<UserDO>, IUserRepository
    {
        internal UserRepository(IUnitOfWork uow)
            : base(uow)
        {
            
        }
        public override void Add(UserDO entity)
        {
            //_curValidator.ValidateAndThrow(entity); //fix this
            base.Add(entity);
/*
            // check default calendar
            UnitOfWork.CalendarRepository.CheckUserHasCalendar(entity);
            // do not send to Admins
            if (entity.Roles.All(r => UnitOfWork.AspNetRolesRepository.GetByKey(r.RoleId).Name != "Admin"))
            {
                AlertManager.CustomerCreated(this.UnitOfWork, entity);
            }
*/
        }

        public UserDO GetByEmailAddress(EmailAddressDO emailAddressDO)
        {
            if (emailAddressDO == null)
                throw new ArgumentNullException("emailAddressDO");
            string fromEmailAddress = emailAddressDO.Address;
            return UnitOfWork.UserRepository.GetQuery().FirstOrDefault(c => c.EmailAddress.Address == fromEmailAddress);
        }

/*
        public UserDO GetOrCreateUser(EmailAddressDO emailAddressDO)
        {
            string fromEmailAddress = emailAddressDO.Address;
            UserRepository userRepo = UnitOfWork.UserRepository;
            UserDO curUser = userRepo.DBSet.Local.FirstOrDefault(c => c.EmailAddress.Address == fromEmailAddress);
            if (curUser == null)
                curUser = userRepo.GetQuery().FirstOrDefault(c => c.EmailAddress.Address == fromEmailAddress);

            if (curUser == null)
            {
                curUser = new UserDO();
                curUser.UserName = fromEmailAddress;
                curUser.FirstName = emailAddressDO.Name;
                curUser.EmailAddress = UnitOfWork.EmailAddressRepository.GetOrCreateEmailAddress(fromEmailAddress);
                userRepo.Add(curUser);
            }
            return curUser;
        }
*/

/*
        public UserDO GetOrCreateUser(BookingRequestDO curMessage)
        {
            string fromEmailAddress = curMessage.From.Address;
            UserRepository userRepo = UnitOfWork.UserRepository;

            UserDO curUser = userRepo.DBSet.Local.FirstOrDefault(c => c.EmailAddress.Address == fromEmailAddress);

            if (curUser == null)
                curUser = userRepo.GetQuery().FirstOrDefault(c => c.EmailAddress.Address == fromEmailAddress);

            if (curUser == null)
            {
                curUser = new UserDO();
                curUser.UserName = fromEmailAddress;
                curUser.FirstName = curMessage.From.Name;
                curUser.EmailAddress = UnitOfWork.EmailAddressRepository.GetOrCreateEmailAddress(fromEmailAddress);
                curMessage.User = curUser;
                userRepo.Add(curUser);
            }
            return curUser;
        }
*/
    }
}