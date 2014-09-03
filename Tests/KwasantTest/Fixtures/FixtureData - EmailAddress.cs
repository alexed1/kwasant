using Data.Entities;
using Data.Interfaces;
using StructureMap;

namespace KwasantTest.Fixtures
{
    partial class FixtureData
    {
        private IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>();

        public EmailAddressDO TestEmailAddress1()
        {
            var emailAddressDO =  uow.EmailAddressRepository.GetOrCreateEmailAddress("alexlucre1@gmail.com", "Alex Lucre1");
            emailAddressDO.Id = 1;
            return emailAddressDO;
        }

        public EmailAddressDO TestEmailAddress2()
        {
            var emailAddressDO = uow.EmailAddressRepository.GetOrCreateEmailAddress("joetest2@edelstein.org", "Joe Test Account 2");
            emailAddressDO.Id = 2;
            return emailAddressDO;
        }
        public EmailAddressDO TestEmailAddress3()
        {
            var emailAddressDO = uow.EmailAddressRepository.GetOrCreateEmailAddress("integrationtesting@kwasant.net", "Kwasant Integration");            
            emailAddressDO.Id = 3;
            return emailAddressDO;
        }
      
        public EmailAddressDO TestEmailAddress4()
        {
            var emailAddressDO = uow.EmailAddressRepository.GetOrCreateEmailAddress("JackMaginot@gmail.com", "Jack Test account");
            emailAddressDO.Id = 4;
            return emailAddressDO;
        }

        public EmailAddressDO TestEmailAddress5()
        {
            var emailAddressDO = uow.EmailAddressRepository.GetOrCreateEmailAddress("RobMaginot@gmail.com", "Jack Test account");
            emailAddressDO.Id = 5;
            return emailAddressDO;
        }
    }
}

