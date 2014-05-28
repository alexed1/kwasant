using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using StructureMap;
using Utilities;
using KwasantCore.Managers.IdentityManager;

namespace KwasantCore.Services
{
    public class Account
    {
        private IdentityManager _identityManager;
        private IUnitOfWork _uow;
        private User _curUser;

        public Account(IUnitOfWork uow) //remove injected uow. unnecessary now.
        {
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();
            _identityManager = new IdentityManager(_uow);
            _curUser = new User(_uow);
        }

        /// <summary>
        /// Register account
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<RegistrationStatus> Register(UserDO userRegStrings)
        {
            RegistrationStatus curRegStatus = RegistrationStatus.Pending;

            //check if we know this email address

            EmailAddressDO existingEmailAddressDO = _uow.EmailAddressRepository.GetQuery().FirstOrDefault(ea => ea.Address == userRegStrings.Email);
            if (existingEmailAddressDO != null)
            {
                //this should be improved. doesn't take advantage of inheritance.
                UserDO curUserDO = _curUser.FindByEmailId(existingEmailAddressDO.Id);
                if (curUserDO != null)
                {
                    
                    if (curUserDO.Password == null)
                    {
                        //this is an existing implicit user, who sent in a request in the past, had a UserDO created, and now is registering. Add the password
                        curUserDO.Password = userRegStrings.Password;
                        _identityManager.AttachPassword(curUserDO);
                    }
                    else
                    {
                        //tell 'em to login
                        curRegStatus = RegistrationStatus.UserMustLogIn;
                    }
                }
                else  //existingEmailAddressDO is Person
                {
                    curRegStatus = RegistrationStatus.Successful;
                }
            }
            else
            {
                //this email address unknown.  new user. create an EmailAddress object, then create a User
                curRegStatus = await _identityManager.RegisterNewUser(userRegStrings);
                curRegStatus = RegistrationStatus.Successful;
            }

            return curRegStatus;
        }

        public async Task<LoginStatus> Login(UserDO userDO, bool isPersistent)
        {
            LoginStatus curLoginStatus = LoginStatus.Pending;

            UserDO user = GetUser(userDO.UserName);
            if (user != null)
            {
                if (user.Password.Length == 0)
                {
                    curLoginStatus = LoginStatus.ImplicitUser;
                }
                else
                {
                    if (user.EmailConfirmed)
                    {
                        userDO.EmailConfirmed = true;
                        curLoginStatus = await _identityManager.Login(userDO, isPersistent);
                    }
                }
            }
            else
            {
                curLoginStatus = LoginStatus.UnregisteredUser;
            }

            return curLoginStatus;
        }

        public void LogOff()
        {
            _identityManager.LogOff();
        }

        /// <summary>
        /// Check user exists or not
        /// </summary>
        /// <param name="KwasantUserDO"></param>
        /// <returns></returns>
        private UserDO GetUser(string userName)
        {
            return _uow.UserRepository.FindOne(x => x.UserName == userName);
        }


    }
}