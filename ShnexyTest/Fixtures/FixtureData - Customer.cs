using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Models;
using NUnit.Framework;
using Shnexy.DDay.iCal;

namespace ShnexyTest.Fixtures
{
    partial class FixtureData
    {




        [Test]
        public Customer TestCustomer()
        {

            return new Customer(customerRepo)
            {
                Id = 1,
                FirstName = "Jack",
                LastName = "Maginot",
                emailAddr = TestEmail1()

            };
        }



    }
}

