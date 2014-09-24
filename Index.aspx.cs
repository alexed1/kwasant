using System;
using System.Web;
using System.Web.UI;
using Data.Entities;
using Data.Interfaces;
using Microsoft.AspNet.Identity;
using StructureMap;
using Utilities;

namespace KwasantWeb
{
    public partial class Index : Page
    {
        private UserDO _currentUser;
        protected void Page_Load(object sender, EventArgs e)
        {
            var userID = HttpContext.Current.User.Identity.GetUserId();
            if (!String.IsNullOrEmpty(userID))
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    _currentUser = uow.UserRepository.GetByKey(userID);
                    var address = _currentUser.EmailAddress; // So it's loaded
                }
            }
        }

        public String GetSegmentWriteKey()
        {
            return new ConfigRepository().Get("SegmentWriteKey");
        }

        public String GetUserID()
        {
            if (_currentUser == null)
                return String.Empty;
            return _currentUser.Id;
        }

        public String GetUserName()
        {
            if (_currentUser == null)
                return String.Empty;
            var returnName = _currentUser.FirstName;
            if (String.IsNullOrEmpty(returnName))
                return _currentUser.LastName;
            if (!String.IsNullOrEmpty(_currentUser.LastName))
                return returnName + " " + _currentUser.LastName;
            return returnName;
        }
        public String GetUserEmail()
        {
            if (_currentUser == null || _currentUser.EmailAddress == null)
                return String.Empty;

            return _currentUser.EmailAddress.Address;
        }
    }
}