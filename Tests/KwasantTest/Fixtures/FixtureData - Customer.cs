using Data.Entities;
using NUnit.Framework;

namespace ShnexyTest.Fixtures
{
    partial class FixtureData
    {
        public CustomerDO TestCustomer()
        {

            return new CustomerDO()
            {
                CustomerID = 1,
                FirstName = "Jack",
                LastName = "Maginot",
                EmailAddress = "JackMaginot@gmail.com"

            };
        }



    }
}

