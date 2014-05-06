using Data.Entities;

namespace KwasantTest.Fixtures
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

        public CustomerDO TestCustomer2()
        {

            return new CustomerDO()
            {
                CustomerID = 2,
                FirstName = "Rob",
                LastName = "Maginot",
                EmailAddress = "RobMaginot@gmail.com"

            };
        }

    }
}

