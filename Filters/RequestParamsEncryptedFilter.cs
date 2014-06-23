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
            if (filterContext.HttpContext.Request.QueryString.AllKeys.Contains(PARAMETER_NAME))
            {
                var encryptedParams = filterContext.HttpContext.Request.QueryString[PARAMETER_NAME];
                var decryptedParams = Encryption.Decrypt(encryptedParams);
                var url = filterContext.HttpContext.Request.RawUrl;
                var urlBuilder = new StringBuilder(url);
                var indexOfParams = url.IndexOf('?') + 1;
                urlBuilder.Remove(indexOfParams, url.Length - indexOfParams);
                urlBuilder.Append(decryptedParams);
                filterContext.Result = new RedirectResult(urlBuilder.ToString());
            }
            else
            {
                this.OnActionExecuting(filterContext);
            }
        }


    }
}