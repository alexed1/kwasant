using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KwasantWeb.Filters
{
    public class RequestParamsEncryptedFilter : ActionFilterAttribute, IActionFilter
    {
        public const string PARAMETER_NAME = "enc";
        private const string ENCRYPTION_KEY = "key";

        void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Request.QueryString.AllKeys.Contains(PARAMETER_NAME))
            {
                var encryptedParams = filterContext.HttpContext.Request.QueryString[PARAMETER_NAME];
                var decryptedParams = Decrypt(encryptedParams);
                foreach (var param in decryptedParams.Split('&'))
                {
                    var parts = param.Split('=');
                    var key = parts[0];
                    var value = parts.Length > 1 ? parts[1] : "";
                    filterContext.ActionParameters[key] = value;
                }
/*
                var url = new StringBuilder(filterContext.HttpContext.Request.RawUrl);
                url.Remove()

                filterContext.Result = new RedirectResult();
*/
            }
            this.OnActionExecuting(filterContext);
        }


        #region Encryption/decryption

        /// <summary>
        /// The salt value used to strengthen the encryption.
        /// </summary>
        private readonly static byte[] SALT = Encoding.ASCII.GetBytes(ENCRYPTION_KEY.Length.ToString());

/*
        public static string Encrypt(IDictionary<string, object> routeValues)
        {
            
        }
*/

        /// <summary>
        /// Encrypts any string using the Rijndael algorithm.
        /// </summary>
        /// <param name="inputText">The string to encrypt.</param>
        /// <returns>A Base64 encrypted string.</returns>
        public static string Encrypt(string inputText)
        {
            RijndaelManaged rijndaelCipher = new RijndaelManaged();
            byte[] plainText = Encoding.Unicode.GetBytes(inputText);
            PasswordDeriveBytes SecretKey = new PasswordDeriveBytes(ENCRYPTION_KEY, SALT);

            using (ICryptoTransform encryptor = rijndaelCipher.CreateEncryptor(SecretKey.GetBytes(32), SecretKey.GetBytes(16)))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(plainText, 0, plainText.Length);
                        cryptoStream.FlushFinalBlock();
                        return WebUtility.UrlEncode(Convert.ToBase64String(memoryStream.ToArray()));
                    }
                }
            }
        }

        /// <summary>
        /// Decrypts a previously encrypted string.
        /// </summary>
        /// <param name="inputText">The encrypted string to decrypt.</param>
        /// <returns>A decrypted string.</returns>
        public static string Decrypt(string inputText)
        {
            RijndaelManaged rijndaelCipher = new RijndaelManaged();
            byte[] encryptedData = Convert.FromBase64String(WebUtility.UrlDecode(inputText));
            PasswordDeriveBytes secretKey = new PasswordDeriveBytes(ENCRYPTION_KEY, SALT);

            using (ICryptoTransform decryptor = rijndaelCipher.CreateDecryptor(secretKey.GetBytes(32), secretKey.GetBytes(16)))
            {
                using (MemoryStream memoryStream = new MemoryStream(encryptedData))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        byte[] plainText = new byte[encryptedData.Length];
                        int decryptedCount = cryptoStream.Read(plainText, 0, plainText.Length);
                        return Encoding.Unicode.GetString(plainText, 0, decryptedCount);
                    }
                }
            }
        }

        #endregion


    }
}