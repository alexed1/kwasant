using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Utilities;

namespace KwasantWeb.Filters
{
    public class RequestParamsEncryptedFilter : ActionFilterAttribute, IActionFilter
    {
        public const string PARAMETER_NAME = "enc";

        void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Request.QueryString.Count == 1)
            {
                // two options:
                // 1) ?enc=<encrypted_params>
                string encryptedParams = filterContext.HttpContext.Request.QueryString[PARAMETER_NAME];
                // 2) ?<encrypted_params>
                if (encryptedParams == null && filterContext.HttpContext.Request.QueryString.GetKey(0) == null)
                {
                    encryptedParams = filterContext.HttpContext.Request.QueryString[0];
                }
                if (!string.IsNullOrEmpty(encryptedParams))
                {
                    try
                    {
                        var decryptedParams = Encryption.Decrypt(encryptedParams);
                        var url = filterContext.HttpContext.Request.RawUrl;
                        var urlBuilder = new StringBuilder(url);
                        var indexOfParams = url.IndexOf('?') + 1;
                        urlBuilder.Remove(indexOfParams, url.Length - indexOfParams);
                        urlBuilder.Append(decryptedParams);
                        filterContext.Result = new RedirectResult(urlBuilder.ToString());
                    }
                    catch (Exception ex)
                    {
                        this.OnActionExecuting(filterContext);
                    }
                }
            }
            else
            {
                this.OnActionExecuting(filterContext);
            }
        }


    }
}