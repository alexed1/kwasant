using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using shnexy.Migrations;
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
            //var configuration = new Configuration();
            //var migrator = new DbMigrator(configuration);
           // migrator.Update();

            //var engine = new Engine();
            //engine.ProcessQueues(); database needs the messagelist initialized from null for this to work 

            Debug.WriteLine("debug output from AdminController");
            ProcessStartInfo info = new ProcessStartInfo("D:/dev/shnexy/ShnexyMTA/bin/Debug/ShnexyMTA.exe");

            using (Process process = Process.Start(info))
            {
                
            }


            return View();
        }
	}
}