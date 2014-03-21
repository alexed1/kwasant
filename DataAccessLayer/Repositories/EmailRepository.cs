using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using Shnexy.DataAccessLayer.Interfaces;
using Shnexy.Models;

namespace Shnexy.DataAccessLayer.Repositories
{


    public class EmailRepository : GenericRepository<Email>,  IEmailRepository
    {

        public EmailRepository(IUnitOfWork uow) : base(uow)
        {
            
        }
    }


    public interface IEmailRepository : IGenericRepository<Email>
    {
        IUnitOfWork UnitOfWork { get; }

        void Add(Email entity);
   
    }
}
