using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Shnexy.Utilities;

namespace Shnexy.Models
{
    public class CallRequest
    {
        public User user;
        public DateTime create_time;
        public CallRequestStatus status;
    }
}
