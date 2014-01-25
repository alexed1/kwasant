using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Shnexy.Models;

namespace Shnexy.ViewModels.User
{
    public class UserDetailsVM
    {
        public string Name;
        public int Id;
        public List<Queue> ProducerFor;
        public List<Queue> ConsumerFor;
        public List<Queue> AvailableFor;
        
    }
}