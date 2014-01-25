using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Shnexy.Models;

namespace Shnexy.Models
{
    public static class Shnesson
    {

        public static List<Queue> Queues; 


        static Shnesson()
        {
            Queues = new List<Queue>();
        }
    }
}