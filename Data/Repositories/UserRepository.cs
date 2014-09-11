using System;
using System.Linq;
using System.Web.UI.WebControls;
using Data.Authentication;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Validations;
using FluentValidation;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using StructureMap;

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

        public UserDO CreateFromEmail(EmailAddressDO emailAddressDO,
            string userName = null, string firstName = null, string lastName = null)
        {
            var um = new UserManager<UserDO>(new UserStore<UserDO>(UnitOfWork.Db as KwasantDbContext));
            var password = Guid.NewGuid().ToString();
            var hashedPassword = um.PasswordHasher.HashPassword(password);

            var curUser = new UserDO
            {
                UserName = userName ?? emailAddressDO.Address,
                FirstName = firstName ?? (emailAddressDO.Name ?? emailAddressDO.Address),
                EmailAddress = emailAddressDO,
                EmailConfirmed = false,
                TestAccount = false,
                PasswordHash = hashedPassword,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            UnitOfWork.UserRepository.Add(curUser);

            var role = new AspNetUserRolesDO();
            role.UserId = curUser.Id;
            role.RoleId = UnitOfWork.AspNetRolesRepository.GetQuery().First(r => r.Name == "Customer").Id;
            UnitOfWork.AspNetUserRolesRepository.Add(role);
            
            UserValidator curUserValidator = new UserValidator();
            curUserValidator.ValidateAndThrow(curUser);
            _uow.UserRepository.Add(curUser);
            _uow.CalendarRepository.CheckUserHasCalendar(curUser);
            return curUser;
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