using Data.Entities;
using Data.Interfaces;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Entities
{
    [TestFixture]
    public class EmailAddressTests : BaseTest
    {
        [Test, ExpectedException(ExpectedMessage = "Duplicate values for 'Address' on 'EmailAddressDO' are not allowed. Duplicated value: 'rjrudman@gmail.com'")]
        public void TestDuplicateEmailRejected()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.EmailAddressRepository.Add(new EmailAddressDO("rjrudman@gmail.com"));
                uow.EmailAddressRepository.Add(new EmailAddressDO("rjrudman@gmail.com"));
                uow.SaveChanges();
            }
        }
    }
}
