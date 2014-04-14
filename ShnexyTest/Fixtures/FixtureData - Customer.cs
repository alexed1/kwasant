using Data.Models;
using NUnit.Framework;

namespace ShnexyTest.Fixtures
{
    partial class FixtureData
    {




        [Test]
        public CustomerDO TestCustomer()
        {

            return new CustomerDO()
            {
                CustomerID = 1,
                FirstName = "Jack",
                LastName = "Maginot",
                Email = TestEmail1()

            };
        }



    }
}

