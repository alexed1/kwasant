using System;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Data.Validations;
using FluentValidation;

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
            base.Add(entity);
            AddDefaultCalendar(entity);
        }

        public UserDO GetByEmailAddress(EmailAddressDO emailAddressDO)
        {
            if (emailAddressDO == null)
                throw new ArgumentNullException("emailAddressDO");
            string fromEmailAddress = emailAddressDO.Address;

            var matchingUser = UnitOfWork.UserRepository.GetQuery().FirstOrDefault(c => c.EmailAddress.Address == fromEmailAddress);
            if (matchingUser == null)
                matchingUser = UnitOfWork.UserRepository.DBSet.Local.FirstOrDefault(c => c.EmailAddress.Address == fromEmailAddress);

            return matchingUser;
        }

        public UserDO CreateFromEmail(EmailAddressDO emailAddressDO,
            string userName = null, string firstName = null, string lastName = null)
        {
            var curUser = new UserDO
            {
                UserName = userName ?? emailAddressDO.Address,
                FirstName = firstName ?? (emailAddressDO.Name ?? emailAddressDO.Address),
                EmailAddress = emailAddressDO,
                EmailConfirmed = false,
                TestAccount = false,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            UnitOfWork.UserRepository.Add(curUser);

            var customerRole = UnitOfWork.AspNetRolesRepository.GetQuery().FirstOrDefault(r => r.Name == "Customer");
            if (customerRole != null)
            {
                var role = new AspNetUserRolesDO();
                role.UserId = curUser.Id;
                role.RoleId = customerRole.Id;
                UnitOfWork.AspNetUserRolesRepository.Add(role);    
            }
            
            UserValidator curUserValidator = new UserValidator();
            curUserValidator.ValidateAndThrow(curUser);
            _uow.UserRepository.Add(curUser);
            _uow.CalendarRepository.CheckUserHasCalendar(curUser);
            return curUser;
        }

        public void AddDefaultCalendar(UserDO curUser)
        {
            if (curUser == null)
                throw new ArgumentNullException("curUser");

            if (!curUser.Calendars.Any())
            {
                var curCalendar = new CalendarDO
                {
                    Name = "Default Calendar",
                    Owner = curUser,
                    OwnerID = curUser.Id
                };
                curUser.Calendars.Add(curCalendar);
            }
        }
    }
}