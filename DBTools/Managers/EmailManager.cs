using System.Collections.Generic;
using System.Diagnostics;
using Data.Models;
using DBTools.Managers.APIManager.Packagers.Mandrill;

namespace DBTools.Managers
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
            string results = MandrillAPI.PostMessageSendTemplate(templateName, message, mergeFields);
        }

        public void Send(Email curEmail)
        {

            string results = MandrillAPI.PostMessageSend(curEmail);
           
        }


        public void Ping()
        {
            string results = MandrillAPI.PostPing();
            Debug.WriteLine(results);
        }

 

        #endregion

      
      
     

    }
}








