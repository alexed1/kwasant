using System.Collections.Generic;
using System.Diagnostics;
using Data.Models;
using Data.Services.APIManager.Packagers.Mandrill;

namespace Data.Services.EmailManager
{
    public class EmailManager
    {
        #region Members

        private MandrillPackager MandrillAPI;

        #endregion

        #region Constructor

        /// <summary>
        /// Initialize EmailManager
        /// </summary>
        public EmailManager()
        {
            MandrillAPI = new MandrillPackager();
        }

        #endregion

        #region Method

        /// <summary>
        /// This implementation of Send uses the Mandrill API
        /// </summary>
        public void SendTemplate(string templateName, Email message, Dictionary<string, string> mergeFields)
        {
            var results = MandrillAPI.PostMessageSendTemplate(templateName, message, mergeFields);
        }

        public void Send(Email curEmail)
        {

            var results = MandrillAPI.PostMessageSend(curEmail);
           
        }


        public void Ping()
        {
            var results = MandrillAPI.PostPing();
            Debug.WriteLine(results);
        }

 

        #endregion

      
      
     

    }
}








