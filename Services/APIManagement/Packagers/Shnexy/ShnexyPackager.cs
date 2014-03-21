using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text.RegularExpressions;
using Shnexy.Services.APIManagement.Serializers.Json;
using Shnexy.Utilities;


namespace Shnexy.Services.APIManagement.Packagers.Shnexy
{
    /// <summary>
    /// This is Shnexy API call Specific class , contains properties which are required for Shnexy API Call.
    /// Contains method for initialize the Restful request
    /// </summary>
    public class ShnexyPackager
    {
        private string baseUrl;
        private JsonSerializer jsonSerializer;

        public ShnexyPackager()
        {
            Initialize(ConfigurationManager.AppSettings["ShnexyApiUrl"]);
            jsonSerializer = new JsonSerializer();
        }

        /// <summary>
        /// Removes trailing slashes from URL and Initializes RestAPIManager
        /// </summary>
        public ShnexyPackager Initialize(string url)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(url);
            baseUrl = url.TrimEnd('/');
            return this;
        }

        /// <summary>
        /// Creates API request, Serializes request object to JSON
        /// Prepares Request Body with JSON Data, Makes rest API call
        /// Deserializes response.
        /// </summary>
        //public string PostLoanApplication(out int loanid, Customer curCustomer, LoanApplicationDO curLoanApplicationDO)
        //{
        //    try
        //    {
        //        RestfulCall curCall = new RestfulCall(baseUrl, "LoanApplication/Process", Method.POST);
        //        string objString = PrepareCreditCheckRequest(curCustomer, curLoanApplicationDO);
        //        curCall.AddQuery("responseString", objString);
        //        curCall.AllowAutoRedirect = false;
        //        RestfulResponse response = curCall.Execute();
        //        //Throw Exception if API Response contains Error Object
        //        try
        //        {
        //            string responseString = response.Content;
        //            string nextJSONObject;
        //            nextJSONObject = GetNextJsonObject(responseString);
        //            curLoanApplicationDO = jsonSerializer.Deserialize<LoanApplicationDO>(nextJSONObject.Replace("#", ""));
        //            loanid = curLoanApplicationDO.LoanId;
        //            nextJSONObject = GetNextJsonObject(responseString.Replace(nextJSONObject, ""));
        //            return nextJSONObject.Replace("#", "");
        //        }
        //        catch
        //        {
        //            throw new ApplicationException(response.Content);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //Throw Exception if calling/Reading API generate error
        //        throw new ApplicationException(ex.ToStr());
        //    }
        //}

 
        /// <summary>
        /// Creates API request
        /// Prepares Request Query with Dictionary param, Makes rest API call to get customer detail from Shnexy
        /// De-serialize response string and returns Customer.
        /// </summary>
        //public List<Customer> GetCustomer(Dictionary<string, string> param)
        //{

        //    try
        //    {
        //        RestfulCall curCall = new RestfulCall(baseUrl, "Customer/Get", Method.GET);
        //        curCall.AddQuery("requestString", jsonSerializer.Serialize(param));
        //        curCall.AllowAutoRedirect = false;
        //        RestfulResponse response = curCall.Execute();
        //        //Throw Exception if API Response contains Error Object
        //        try
        //        {
        //            return jsonSerializer.Deserialize<List<Customer>>(response.Content);
        //        }
        //        catch
        //        {
        //            throw new ApplicationException(response.Content);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //Throw Exception if calling/Reading API generate error
        //        throw new ApplicationException(ex.ToStr());

        //    }
        //}

        /// <summary>
        /// Creates API request
        /// Prepares Request Query with Dictionary param, Makes rest API call to update customer detail into Shnexy
        /// De-serialize response string and returns Customer.
        /// </summary>
        //public Customer PostCustomer(Customer ShnexyCustomer)
        //{
        //    RestfulCall curCall = new RestfulCall(baseUrl, "Customer/Post", Method.POST);
        //    string customerData = jsonSerializer.Serialize(ShnexyCustomer);
        //    curCall.AddBody(customerData, "application/json");
        //    curCall.AllowAutoRedirect = false;
        //    // TODO : ShnexyAPI is not implemented, after implementation uncomment following code and call ShnexyAPI
        //    //var response = curCall.Execute();
        //    //return jsonSerializer.Deserialize<Customer>(response.Content);
        //    return new Customer();
        //}

        /// <summary>
        /// Get Json Object string from response using Regex
        /// </summary>
        public string GetNextJsonObject(string jsonString)
        {
            Regex rgx = new Regex(@"\#(.*?)\#");
            return rgx.Matches(jsonString)[0].ToString();
        }


        #region UnPack Method

        /// <summary>
        /// De-serialize Request string into specific object
        /// </summary>
        public void UnpackGetCustomer(string requestString, out Dictionary<string, string> param)
        {
            param = jsonSerializer.Deserialize<Dictionary<string, string>>(requestString);
            string ShnexyCustomerId = "";
            param.TryGetValue("ShnexyCustomerId", out ShnexyCustomerId);
            if (ShnexyCustomerId == "" || ShnexyCustomerId == null)
                throw new ArgumentException("Call to GetCustomer must provide CustomerId", "Required : ShnexyCustomerId");
            if (ShnexyCustomerId.ToInt() <= 0)
                throw new ArgumentException("CustomerId must be positive", "Invalid : ShnexyCustomerId");
            if (param.ContainsKey("EmailAddress") || param.ContainsKey("FirstName") || param.ContainsKey("LastName") || param.ContainsKey("SSN"))
                throw new ArgumentException("The LIKI implementation of GET /customer currently only supports the parameter key CustomerID", "");
            if (param.ContainsKey("Mode"))
            {
                string mode = "";
                param.TryGetValue("Mode", out mode);
                if (mode != "Partial")
                    throw new ArgumentException("Call to GetCustomer must provide Mode, and it must be 'Partial'", "Invalid : Mode");
            }
            if (param.ContainsKey("Application_Type"))
                param["Application_Type"] = "multimerchant";
            else
                param.Add("Application_Type", "multimerchant");
        }

        /// <summary>
        /// De-serialize Request string into Customer object
        /// </summary>
        //public void UnpackPostCustomer(string requestString, out Customer curCustomer)
        //{
        //    curCustomer = jsonSerializer.Deserialize<Customer>(requestString);
        //    if (curCustomer == null)
        //        throw new ArgumentException("Call to PostCustomer must provide Customer Object", "Required : Customer Object");
        //}


        ///// <summary>
        ///// De-serialize Request string into Dictionary object
        ///// validate parameters
        ///// </summary>
        //public void UnpackGetProductLease(string requestString, out Dictionary<string, string> param)
        //{
        //    bool sucess = false;
        //    param = jsonSerializer.Deserialize<Dictionary<string, string>>(requestString);
        //    if (param == null)
        //        throw new ArgumentException("Failed to process Get call as Param object is null", "Required : Param Object");
        //    if (param.ContainsKey("CustomerId"))
        //    {
        //        sucess = true;
        //        if (!param["CustomerId"].IsNumeric())
        //            throw new ArgumentException("CustomerId must be numeric", "Invalid : CustomerId");
        //        if (param["CustomerId"].ToInt() <= 0)
        //            throw new ArgumentException("CustomerId must be positive", "Invalid : CustomerId");
        //    }
        //    else if (param.ContainsKey("Id"))
        //    {
        //        sucess = true;
        //        if (!param["Id"].IsNumeric())
        //            throw new ArgumentException("Id must be numeric", "Invalid : Id");
        //        if (param["Id"].ToInt() <= 0)
        //            throw new ArgumentException("Id must be positive", "Invalid : Id");
        //    }

        //    //raise exception if either CustomerId or Id is not one of the parameter in param object
        //    if (!sucess)
        //    {
        //        throw new ArgumentException("Call to process GET/Product Lease required either CustomerId or Id as one of the parameter", "");
        //    }
        //}

        ///// <summary>
        ///// De-serialize Request string into specific object
        ///// </summary>
        //public void UnpackGetMerchant(string requestString, out Dictionary<string, string> param)
        //{
        //    param = jsonSerializer.Deserialize<Dictionary<string, string>>(requestString);
        //    if (param == null)
        //        throw new ArgumentException("Failed to process Get call as Param object is null", "Required : Param Object");

        //    int merchantId = param["Id"].ToInt();
        //    if (merchantId <= 0)
        //        throw new ArgumentException("Merchant Id must be positive", "Invalid : Id");
        //}

        #endregion

        #region Pack Method

        /// <summary>
        /// Serialize Response object into JSON string
        /// </summary>
        //public void PackResponseGetcustomer(List<Customer> curCustomerListDO, out string result)
        //{
        //    result = jsonSerializer.Serialize(curCustomerListDO);
        //}

        ///// <summary>
        ///// Serialize postCustomer status into JSON string
        ///// </summary>
        //public void PackResponsePostCustomer(bool status, out string result)
        //{
        //    result = jsonSerializer.Serialize(status.ToString());
        //}

        /// <summary>
        /// Serialize ArgumentException type error response into JSON String
        /// </summary>
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

        /// <summary>
        /// Serialize Product Lease List object into JSON string
        /// </summary>
        //public void PackResponseGetProductLease(List<ProductLeaseDO> curProductLeaseDOList, out string result)
        //{
        //    result = jsonSerializer.Serialize(curProductLeaseDOList);
        //}

        ///// <summary>
        ///// Serialize Merchant object into JSON string
        ///// </summary>
        //public void PackResponseMerchant(MerchantDO curMerchantDO, out string result)
        //{
        //    result = jsonSerializer.Serialize(curMerchantDO);
        //}

        #endregion
    }

    public class Error
    {
        public string Name { get; set; }
        public string Message { get; set; }
    }
}
