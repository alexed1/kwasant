using System.Collections.Generic;
using System.Diagnostics;
using Data.Models;
using Data.Tools.Managers.APIManager.Packagers.Mandrill;

namespace Data.Tools.Managers
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
        public void SendTemplate(string templateName, EmailDO message, Dictionary<string, string> mergeFields)
        {
            string results = MandrillAPI.PostMessageSendTemplate(templateName, message, mergeFields);
        }

        public void Send(EmailDO curEmailDO)
        {

            string results = MandrillAPI.PostMessageSend(curEmailDO);
           
        }


        public void Ping()
        {
            string results = MandrillAPI.PostPing();
            Debug.WriteLine(results);
        }

 

        #endregion

      
      
     

    }
}








