using System;
using System.Security.Claims;
using Data.Entities;
using Data.Interfaces;

namespace KwasantCore.Security
{
    public interface ISecurityServices
    {
        void Login(IUnitOfWork uow, UserDO userDO);
        String GetCurrentUser();
        String GetUserName();
        bool IsAuthenticated();
        void Logout();
    }
}