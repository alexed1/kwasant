using Data.Models;


namespace Data.DataAccessLayer.Fixtures
{
    public partial class FixtureData
    {




       
        public Customer TestCustomer()
        {

            return new Customer(customerRepo)
            {
                Id = 1,
                FirstName = "Jack",
                LastName = "Maginot",
                emailAddr = TestEmailAddress1()

            };
        }



    }
}

