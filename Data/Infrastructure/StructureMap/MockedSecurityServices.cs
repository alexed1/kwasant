using System;
using Data.Entities;
using Data.Interfaces;

namespace Data.Infrastructure.StructureMap
{
    public class MockedSecurityServices : ISecurityServices
    {
        private readonly object _locker = new object();
        private UserDO _currentLoggedInUser;
        public void Login(IUnitOfWork uow, UserDO userDO)
        {
            lock (_locker)
                _currentLoggedInUser = userDO;
        }

        public String GetCurrentUser()
        {
            lock (_locker)
                return _currentLoggedInUser == null ? String.Empty : _currentLoggedInUser.Id;
        }

        public string GetUserName()
        {
            lock (_locker)
                return _currentLoggedInUser == null ? String.Empty : (_currentLoggedInUser.FirstName + " " + _currentLoggedInUser.LastName);
        }

        public bool IsAuthenticated()
        {
            lock (_locker)
                return !String.IsNullOrEmpty(GetCurrentUser());
        }

        public void Logout()
        {
            lock (_locker)
                _currentLoggedInUser = null;
        }
    }
}
