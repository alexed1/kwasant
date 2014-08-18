using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Data.Infrastructure;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Validators;
using FluentValidation;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using System.Web;
using Data.Entities;
using StructureMap;
using Utilities;

namespace KwasantCore.Services
{
    public class User
    {
        private static IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.Current.GetOwinContext().Authentication;
            }
        }

        public UserDO GetOrCreate(IUnitOfWork uow, EmailAddressDO emailAddressDO)
        {
            if (uow == null)
                throw new ArgumentNullException("uow");
            if (emailAddressDO == null)
                throw new ArgumentNullException("emailAddressDO");
            UserDO curUser = Get(uow, emailAddressDO);
            if (curUser == null)
            {
                var id = Create(emailAddressDO.Address);
                curUser = uow.UserRepository.GetByKey(id);
            }
            return curUser;
        }

        public UserDO Get(IUnitOfWork uow, BookingRequestDO bookingRequestDO)
        {
            if (bookingRequestDO == null)
                throw new ArgumentNullException("bookingRequestDO");
            return Get(uow, bookingRequestDO.From);
        }

        public UserDO Get(IUnitOfWork uow, EmailAddressDO emailAddressDO)
        {
            if (uow == null)
                throw new ArgumentNullException("uow");
            if (emailAddressDO == null)
                throw new ArgumentNullException("emailAddressDO");
            return uow.UserRepository.GetByEmailAddress(emailAddressDO);
        }

        /// <summary>
        /// Creates a user with passed email address in a separate UnitOfWork.
        /// </summary>
        /// <returns>
        /// Returns created user's ID.
        /// </returns>
        /// <remarks>
        /// Doesn't return the created user entity object as anyway it would belong to closed UnitOfWork instantiated inside.
        /// </remarks>
        /// <param name="emailAddress"></param>
        public string Create(string emailAddress)
        {
            if (string.IsNullOrEmpty(emailAddress))
                throw new ArgumentNullException("emailAddress");
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curEmailAddress = uow.EmailAddressRepository.GetOrCreateEmailAddress(emailAddress);
                var curUser = Create(uow, curEmailAddress);
                uow.SaveChanges();
                AlertManager.CustomerCreated(curUser);
                return curUser.Id;
            }
        }

        private UserDO Create(IUnitOfWork uow, EmailAddressDO emailAddressDO,
            string userName = null, string firstName = null, string lastName = null)
        {
            Debug.Assert(uow != null);
            Debug.Assert(emailAddressDO != null);
            var curUser = new UserDO
            {
                UserName = userName ?? emailAddressDO.Address,
                FirstName = firstName ?? emailAddressDO.Name,
                LastName = lastName ?? string.Empty,
                EmailAddress = emailAddressDO
            };
            UserValidator curUserValidator = new UserValidator();
            curUserValidator.ValidateAndThrow(curUser);
            uow.UserRepository.Add(curUser);
            uow.CalendarRepository.CheckUserHasCalendar(curUser);
            return curUser;
        }

        public UserDO Register (IUnitOfWork uow, string userName, string password, string role)
        {
            var userDO = Create(uow,
                emailAddressDO: uow.EmailAddressRepository.GetOrCreateEmailAddress(userName),
                userName: userName,
                firstName: userName,
                lastName: userName);

            UserManager<UserDO> userManager = GetUserManager(uow);;
            IdentityResult result = userManager.Create(userDO, password);
            if (result.Succeeded)
            {
                userManager.AddToRole(userDO.Id, role);
            }
            else
            {
                throw new ApplicationException("There was a problem trying to register you. Please try again.");
            }

            return userDO;
        }

        public void UpdatePassword(IUnitOfWork uow, UserDO userDO, string password)
        {
            if (userDO != null)
            {
                UserManager<UserDO> curUserManager = GetUserManager(uow);;

                curUserManager.RemovePassword(userDO.Id); //remove old password
                var curResult = curUserManager.AddPassword(userDO.Id, password); // add new password
                if (curResult.Succeeded == false)
                {
                    throw new ApplicationException("There was a problem trying to change your password. Please try again.");
                }
            }
        }

        public async Task<LoginStatus> Login(IUnitOfWork uow, string username, string password, bool isPersistent)
        {
            LoginStatus curLogingStatus = LoginStatus.Successful;
            UserManager<UserDO> curUserManager = GetUserManager(uow);;
            UserDO curUser = await curUserManager.FindAsync(username, password);
            if (curUser != null)
            {
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);

                ClaimsIdentity identity = await curUserManager.CreateIdentityAsync(curUser, DefaultAuthenticationTypes.ApplicationCookie);

                if (identity.IsAuthenticated == false)
                {
                    throw new ApplicationException("There was an error logging in. Please try again later.");
                }
                AuthenticationManager.SignIn(new AuthenticationProperties
                {
                    IsPersistent = isPersistent
                }, identity);
            }
            else
            {
                curLogingStatus = LoginStatus.InvalidCredential;
            }

            return curLogingStatus;
        }

        public void LogOff()
        {
            AuthenticationManager.SignOut();
        }
        
        //problem: this assumes a single role but we need support for multiple roles on one account
        //problem: the line between account and user is really murky. do we need both?
        public bool ChangeUserRole(IUnitOfWork uow, IdentityUserRole identityUserRole)
        {
            UserManager<UserDO> userManager = GetUserManager(uow);
            RoleManager<IdentityRole> roleManager = Role.GetRoleManager(uow);

            IList<string> currCurrentIdentityRole = userManager.GetRoles(identityUserRole.UserId);
            IdentityResult identityResult = userManager.RemoveFromRole(identityUserRole.UserId, currCurrentIdentityRole.ToList()[0]);

            if (identityResult.Succeeded)
            {
                IdentityRole currNewIdentityRole = roleManager.FindById(identityUserRole.RoleId.Trim());
                identityResult = userManager.AddToRole(identityUserRole.UserId.Trim(), currNewIdentityRole.Name);
            }

            return identityResult.Succeeded;
        }

        public static UserManager<UserDO> GetUserManager(IUnitOfWork uow)
        {
            var userStore = ObjectFactory.GetInstance<IKwasantUserStore>();
            var um = new UserManager<UserDO>(userStore.SetUnitOfWork(uow));
            var provider = new Microsoft.Owin.Security.DataProtection.DpapiDataProtectionProvider("Sample");
            um.UserTokenProvider = new Microsoft.AspNet.Identity.Owin.DataProtectorTokenProvider<UserDO>(provider.Create("EmailConfirmation"));

            return um;
        }

        public List<CalendarDO> GetCalendars(string curUserId) 
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return uow.CalendarRepository.GetAll().Where(e => e.OwnerID == curUserId).Select(e => new CalendarDO { Id = e.Id, Name = e.Name }).ToList();
            }
        }

        public UserDO GetFirstUser()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curUserDO = uow.UserRepository.GetAll().First();
                return new UserDO()
                {
                    Calendars = curUserDO.Calendars,
                    AccessFailedCount = curUserDO.AccessFailedCount,
                    Email = curUserDO.Email,
                    BookingRequests = curUserDO.BookingRequests,
                    Id = curUserDO.Id,
                    EmailAddress = curUserDO.EmailAddress
                };
            }
        }
    }
}
