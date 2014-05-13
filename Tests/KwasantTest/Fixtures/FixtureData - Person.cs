using Data.Entities;

namespace KwasantTest.Fixtures
{
    partial class FixtureData
    {
        public PersonDO TestPerson1()
        {

            return new PersonDO()
            {
                FirstName="Test First Name",
                LastName = "Test Last Name"                
            };
        }       
    }
}

