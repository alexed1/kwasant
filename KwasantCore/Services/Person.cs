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

        public void Delete(string userName)
        {
            KwasantDbContext curContext = _personRepo.UnitOfWork.Db as KwasantDbContext;
            if (curContext != null)
            {
                PersonDO personDO = _personRepo.FindOne(e => e.EmailAddress.Address == userName);
                if (personDO != null)
                {
                    _personRepo.Remove(personDO);
                }
            }
        }
    }
}
