using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using Shnexy.DataAccessLayer.Interfaces;
using Shnexy.Models;

namespace Shnexy.DataAccessLayer.Repositories
{

    public class EmailAddressRepository : GenericRepository<EmailAddress>, IEmailAddressRepository
    {

        public EmailAddressRepository(IUnitOfWork uow)
            : base(uow)
        {
            
        }
    }


    public interface IEmailAddressRepository
    {
        IUnitOfWork UnitOfWork { get; }
        EmailAddress GetByKey(object keyValue);
        IQueryable<EmailAddress> GetQuery();
        void Add(EmailAddress entity); 
        void Remove(EmailAddress entity);
        void Attach(EmailAddress entity);
        IEnumerable<EmailAddress> GetAll();
        void Save(EmailAddress entity);
        void Update(EmailAddress entity, EmailAddress existingEntity);
        EmailAddress FindOne(Expression<Func<EmailAddress, bool>> criteria);
        IEnumerable<EmailAddress> FindList(Expression<Func<EmailAddress, bool>> criteria);
        void Dispose();
    }
}
