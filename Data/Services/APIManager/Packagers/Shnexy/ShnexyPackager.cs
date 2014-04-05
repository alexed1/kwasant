using System;
using System.Collections.Generic;
using Data.Models;
using Data.Services.APIManager.Serializers.Json;
using UtilitiesLib;

namespace Data.Services.APIManager.Packagers.Shnexy
{
    /// <summary>
    /// This class converts JSON to Objects and vice versa, so that RESTful calls can be processed.
    /// </summary>
    public class ShnexyPackager
    {
        private string baseUrl;
        private JsonSerializer jsonSerializer;

        public ShnexyPackager()
        {
           
            jsonSerializer = new JsonSerializer();
        }



        #region UnPack Method

        /// <summary>
        /// De-serialize GET /Email
        /// </summary>
        public void UnpackGetEmail(string requestString, out Dictionary<string, string> param)
        {
            param = jsonSerializer.Deserialize<Dictionary<string, string>>(requestString);
            string EmailId = "";
            param.TryGetValue("EmailId", out EmailId);
            if (EmailId == "" || EmailId == null)
                throw new ArgumentException("Call to Get Email must provide an Email ID", "Required : Email Id");
            if (EmailId.ToInt() <= 0)
                throw new ArgumentException("EmailId must be positive", "Invalid : EmailId");
           
        }

        public string PackResponseGetEmail(Email curEmail)
        {
          return jsonSerializer.Serialize(curEmail);
           
        }

      
        #endregion

        #region Pack Method

      
        public void PackErrorMessage(ArgumentException ex, out string result)
        {
            Error curError = new Error();
            if (ex.ParamName != null)
            {
                curError.Name = ex.ParamName;
                curError.Message = ex.Message;
            }
            else
            {
                curError.Name = "Shnexy API Error";
                curError.Message = "The call to the API could not be processed";
            }
            result = jsonSerializer.Serialize(curError);
        }

        /// <summary>
        /// Serialize ApplicationException type error response into JSON String
        /// </summary>
        public void PackErrorMessage(ApplicationException ex, out string result)
        {
            Error curError = new Error();
            curError.Name = "LIKI API Error";
            curError.Message = ex.Message;
            result = jsonSerializer.Serialize(curError);
        }

        
        #endregion
    }

    public class Error
    {
        public string Name { get; set; }
        public string Message { get; set; }
    }
}
