using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using shnexy.Migrations;
using System.Data.Entity.Migrations;
using Shnexy.Models;

namespace Shnexy.Controllers
{
    public class AdminController : Controller
    {
        //
        // GET: /Admin/
        public ActionResult Index()
        {
            var configuration = new Configuration();
            var migrator = new DbMigrator(configuration);
            migrator.Update();

            var engine = new Engine();
            engine.ProcessQueues();
            return View();
        }
	}
}