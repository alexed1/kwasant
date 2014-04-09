using Data.Models;
using NUnit.Framework;

namespace ShnexyTest.Fixtures
{
    partial class FixtureData
    {




        [Test]
        public Customer TestCustomer()
        {

            return new Customer()
            {
                CustomerID = 1,
                FirstName = "Jack",
                LastName = "Maginot",
                Email = TestEmail1()

            };
        }



    }
}

