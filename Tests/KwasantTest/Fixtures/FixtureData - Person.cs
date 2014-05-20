using Data.Entities;
using NUnit.Framework;

namespace KwasantTest.Fixtures
{
    partial class FixtureData
    {
        [Test]
        public PersonDO TestPerson()
        {
            return new PersonDO()
            {
                FirstName = "Pabitra",
                EmailAddress = TestEmail3()
            };
        }
    }
}

