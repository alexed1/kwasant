using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Shnexy.Utilities;

namespace Shnexy.Models
{
    public class Call
    {
        public List<User> Producers;
        public List<User> Consumers;
        public CallState State;
        public Topic Topic;
    }
}