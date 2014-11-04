using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace KwasantTest.Controllers
{
    class ReportControllerTests
    {
        [Test]
        public void TestMenuItem_ReportsShowIncidents()
        {
            WebRequest webRequest = WebRequest.Create("https://www.kwasant.com/Report?type=incident");
            var response = (HttpWebResponse)webRequest.GetResponse();
            Assert.AreEqual("OK", response.StatusCode.ToString());
        }

        [Test]
        public void TestMenuItem_ReportsShowHistory()
        {
            WebRequest webRequest = WebRequest.Create("https://www.kwasant.com/Report/History");
            var response = (HttpWebResponse)webRequest.GetResponse();
            Assert.AreEqual("OK", response.StatusCode.ToString());
        }
    }
}
