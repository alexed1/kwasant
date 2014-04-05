using Data.Models;
using NUnit.Framework;

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

