using Data.Entities;
using Data.Interfaces;

namespace KwasantCore.Services
{
    public class Person
    {
        private readonly IUnitOfWork _uow;

        public Person(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public void Delete(PersonDO curPersonDO)
        {
            if (curPersonDO != null)
            {
                _uow.PersonRepository.Remove(curPersonDO);
            }
        }

        public PersonDO GetByKey(int Id)
        {
            return _uow.PersonRepository.GetByKey(Id);

        }

        public PersonDO FindByEmailId(int Id)
        {
            return _uow.PersonRepository.FindOne(p => p.EmailAddress.Id == Id);

        }
    }
}
