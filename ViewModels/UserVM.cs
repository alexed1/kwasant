using System;

namespace KwasantWeb.ViewModels
{
    public class UserVM
    {
        public String Id { get; set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public String UserName { get; set; }
        public String EmailAddress { get; set; }
        public int EmailAddressID { get; set; }
        public String RoleName { get; set; }
        public String RoleId { get; set; }
    }
}