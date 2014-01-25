using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Shnexy.Utilities
{
    public static class ObjectExtension
    {
        public static string to_S(this object value)
        {
            return value.ToString();
        }
    }
}