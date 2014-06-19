using System.Collections.Generic;
using AutoMapper;
using KwasantCore.Services;
using ViewModel.Models;

namespace KwasantWeb.Controllers.Helpers
{
    public class ObjectMapper
    {
        public static List<UsersAdminViewModel> GetMappedUsersAdminViewModelList(List<UsersAdminData> usersAdminDataList)
        {
            List<UsersAdminViewModel> currUsersAdminViewModels = new List<UsersAdminViewModel>();

            Mapper.CreateMap<UsersAdminData, UsersAdminViewModel>();

            foreach (UsersAdminData usersAdminData in usersAdminDataList)
            {
                UsersAdminViewModel usersAdminViewModel = Mapper.Map<UsersAdminData, UsersAdminViewModel>(usersAdminData);
                currUsersAdminViewModels.Add(usersAdminViewModel);
            }

            //Func<UsersAdminViewModel, string> orderingFunction = (c => c.FirstName);
            //currUsersAdminViewModels.OrderByDescending(orderingFunction);

            return currUsersAdminViewModels;
        }

    }
}