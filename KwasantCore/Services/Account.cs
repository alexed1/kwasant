using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using StructureMap;
using Utilities;
using KwasantCore.Managers;
using KwasantCore.Managers.IdentityManager;
using KwasantCore.Managers.CommunicationManager;

namespace KwasantCore.Services
{
    public class Account
    {
        private IdentityManager _identityManager;
        private IUnitOfWork _uow;
        private User _curUser;
        private CommunicationManager _commManager;

        public Account(IUnitOfWork uow) //remove injected uow. unnecessary now.
        {
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();
            _identityManager = new IdentityManager(_uow);
            _curUser = new User(_uow);
            _commManager = new CommunicationManager();
        }

        /// <summary>
        /// Register account
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public  RegistrationStatus Register(UserDO userRegStrings)
        {
            RegistrationStatus curRegStatus = RegistrationStatus.Pending;
            UserDO curUserDO = null;
            //check if we know this email address
            
            EmailAddressDO existingEmailAddressDO = _uow.EmailAddressRepository.GetQuery().FirstOrDefault(ea => ea.Address == userRegStrings.Email);
            if (existingEmailAddressDO != null)
            {
                
                 curUserDO = _curUser.FindByEmailId(existingEmailAddressDO.Id);
                if (curUserDO != null)
                {
                    if (curUserDO.Password == null)
                    {
                        //this is an existing implicit user, who sent in a request in the past, had a UserDO created, and now is registering. Add the password
                        curUserDO.Password = userRegStrings.Password;
                        _identityManager.AttachPassword(curUserDO);
                        curRegStatus = RegistrationStatus.Successful;
                    }
                    else
                    {
                        //tell 'em to login
                        curRegStatus = RegistrationStatus.UserMustLogIn;
                    }
                }
            }
            else
            {
                //this email address unknown.  new user. create an EmailAddress object, then create a User
                
                curUserDO =  _curUser.Register(userRegStrings, "Customer");
                curRegStatus = RegistrationStatus.Successful;
                
            }

            if (curRegStatus == RegistrationStatus.Successful)
                AlertManager.CustomerCreated(curUserDO);
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