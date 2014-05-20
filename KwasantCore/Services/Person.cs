using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Data.Entities;
using Data.Repositories;
using Data.Interfaces;
using Data.Infrastructure;

namespace KwasantCore.Services
{
    public class Person
    {
        private PersonRepository _personRepo;

        public Person(IUnitOfWork uow)
        {
            _personRepo = new PersonRepository(uow);  
        }

        public void Delete(PersonDO curPersonDO)
        {
            if (curPersonDO != null)
            {
                _personRepo.Remove(curPersonDO);
            }
        }

        public PersonDO GetByKey(int Id)
        {
            return _personRepo.GetByKey(Id);

        }

        public PersonDO FindByEmailId(int Id)
        {
            return _personRepo.FindOne(p => p.EmailAddress.Id == Id);

        }
    }
}
