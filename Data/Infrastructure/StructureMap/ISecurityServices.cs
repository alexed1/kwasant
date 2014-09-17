using System;
using Data.Entities;
using Data.Interfaces;

namespace Data.Infrastructure.StructureMap
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