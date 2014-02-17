using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Shnexy.Models
{
    public class EngineController : Controller
    {

        public ActionResult ProcessQueues()
        {

            Engine curEngine = new Engine();
            curEngine.ProcessQueues();
            return View();
        }
	}
}