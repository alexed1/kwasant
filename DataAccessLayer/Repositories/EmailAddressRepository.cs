using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using Shnexy.DataAccessLayer.Interfaces;
using Shnexy.Models;

namespace Shnexy.DataAccessLayer.Repositories
{


    public class EmailAddressRepository : GenericRepository<Email>,  IEmailAddressRepository
    {

        public EmailAddressRepository(IUnitOfWork uow) : base(uow)
        {
            
        }
    }


    public interface IEmailAddressRepository : IGenericRepository<Email>
    {
        IUnitOfWork UnitOfWork { get; }

      
   
    }
}
