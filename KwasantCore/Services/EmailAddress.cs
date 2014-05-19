using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using StructureMap;

namespace KwasantCore.Services
{
    internal class EmailAddress
    {
        private EmailAddressRepository _emailAddressRepository;
        private IUnitOfWork _uow;

        public EmailAddress()
        {
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();
            _emailAddressRepository = new EmailAddressRepository(_uow);
        }

        public EmailAddressDO FindByAddress(string addressString)
        {
            return _emailAddressRepository.FindOne(x => x.Address == addressString);
        }
    }


}
