using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace KwasantWeb.ViewModels
{
    /// <summary>
    /// This is the VM used to create the VIEW /Email/Send
    /// </summary>
    public class CreateEmailVM : SendEmailVM
    {
        public CreateEmailVM()
        {
            AddressBook = new List<string>();
            InsertLinks = new List<InsertLink>();
        }
        public IList<String> AddressBook { get; set; }
        public IList<InsertLink> InsertLinks { get; set; }

        public class InsertLink
        {
            public String Id { get; set; }
            public String DisplayName { get; set; }
            public String TextToInsert { get; set; }
        }
    }

    /// <summary>
    /// This is the VM used to actually send the email (The data /Email/Send sends us)
    /// </summary>
    public class SendEmailVM
    {
        public SendEmailVM()
        {
            ToAddresses = new List<string>();
            CCAddresses = new List<string>();
            BCCAddresses = new List<string>();
        }

        public String CallbackToken { get; set; }

        public IList<String> ToAddresses { get; set; }
        public IList<String> CCAddresses { get; set; }
        public IList<String> BCCAddresses { get; set; }

        public String Subject { get; set; }
        public String Body { get; set; }
    }
}