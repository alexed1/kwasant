using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Text;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Web.Security;

using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using Data.Infrastructure;
using ViewModel.Models;
using KwasantCore.Services;
using KwasantWeb.Controllers.Helpers;

namespace KwasantWeb.Controllers
{
    public class UserController : Controller
    {
        IUnitOfWork _uow;
        IUserRepository userRepo;
        IAspNetUserRolesRepository aspNetUserRolesRepo;
        IAspNetRolesRepository aspNetRolesRepo;

        public UserController(IUnitOfWork uow)
        {
            _uow = uow;
            userRepo = _uow.UserRepository;
            aspNetUserRolesRepo = _uow.AspNetUserRolesRepository;
            aspNetRolesRepo = _uow.AspNetRolesRepository;
        }

        public ActionResult Index()
        {
            UsersAdmin currUsersAdmin = new UsersAdmin();
            List<UsersAdminData> currUsersAdminDataList = currUsersAdmin.GetUsersAdminViewData();
            List<UsersAdminViewModel> currUsersAdminViewModels = currUsersAdminDataList != null && currUsersAdminDataList.Count > 0 ? ObjectMapper.GetMappedUsersAdminViewModelList(currUsersAdminDataList) : null;

            return View(currUsersAdminViewModels);
        }

        public ActionResult Details(String userId, String roleId)
        {

            if (String.IsNullOrEmpty(userId) || String.IsNullOrEmpty(roleId))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            UsersAdmin currUsersAdmin = new UsersAdmin();
            List<UsersAdminData> currUsersAdminDataList = currUsersAdmin.GetUsersAdminViewData(userId, roleId);

            List<UsersAdminViewModel> currUsersAdminViewModels = currUsersAdminDataList != null && currUsersAdminDataList.Count > 0 ? ObjectMapper.GetMappedUsersAdminViewModelList(currUsersAdminDataList) : null;

            UsersAdminViewModel currUsersAdminViewModel = currUsersAdminViewModels == null || currUsersAdminViewModels.Count == 0 ? new UsersAdminViewModel() : currUsersAdminViewModels[0];

            return View(currUsersAdminViewModel);
        }
    }
}

