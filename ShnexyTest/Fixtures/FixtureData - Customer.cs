using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Shnexy.DataAccessLayer.Interfaces;
using Shnexy.DataAccessLayer.Repositories;
using Shnexy.DataAccessLayer.StructureMap;
using Shnexy.Models;
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
                email = new EmailAddress
                {
                    Email = "alexlucre1@gmail.com",
                    Id = 1,
                    Name = "Alex Lucre1"
                }

            };
        }



    }
}

