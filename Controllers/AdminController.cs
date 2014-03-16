using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.Data.Entity.Migrations;
using Shnexy.Models;
using System.Diagnostics;
using System.IO;

namespace Shnexy.Controllers
{
    public class AdminController : Controller
    {
        //
        // GET: /Admin/
        public ActionResult Index()
        {


            //var engine = new Engine();
            //engine.ProcessQueues(); database needs the messagelist initialized from null for this to work 



            return View();
        }
	}
}