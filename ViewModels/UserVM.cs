using System;
using System.Collections.Generic;

namespace KwasantWeb.ViewModels
{
    public class UserVM
    {
        public UserVM()
        {
            Roles = new List<String>();
            Calendars = new List<UserCalendarVM>();
        }

        public String Id { get; set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public String UserName { get; set; }
        public String EmailAddress { get; set; }
        public int EmailAddressID { get; set; }

        public List<String> Roles { get; set; } 
        public List<UserCalendarVM> Calendars { get; set; }
    }

    public class UserCalendarVM
    {
        public int Id { get; set; }
        public String Name { get; set; }
    }
}