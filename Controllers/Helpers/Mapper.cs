using System.Collections.Generic;
using AutoMapper;
using KwasantCore.Services;
using ViewModel.Models;

namespace KwasantWeb.Controllers.Helpers
{
    public class ObjectMapper
    {
        public static List<UsersAdminVM> GetMappedUsersAdminVMList(List<UsersAdminData> usersAdminDataList)
        {
            List<UsersAdminVM> currUsersAdminVMs = new List<UsersAdminVM>();

            Mapper.CreateMap<UsersAdminData, UsersAdminVM>();

            foreach (UsersAdminData usersAdminData in usersAdminDataList)
            {
                UsersAdminVM usersAdminVM = Mapper.Map<UsersAdminData, UsersAdminVM>(usersAdminData);
                currUsersAdminVMs.Add(usersAdminVM);
            }

            //Func<UsersAdminViewModel, string> orderingFunction = (c => c.FirstName);
            //currUsersAdminViewModels.OrderByDescending(orderingFunction);

            return currUsersAdminVMs;
        }

    }
}