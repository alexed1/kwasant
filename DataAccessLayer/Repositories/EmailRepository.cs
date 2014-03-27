using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using Shnexy.DataAccessLayer.Interfaces;
using Shnexy.Models;

namespace Shnexy.DataAccessLayer.Repositories
{

    public class EmailRepository : GenericRepository<Email>, IEmailRepository
    {

        public EmailRepository(IUnitOfWork uow) : base(uow)
        {
            
        }
    }


    public interface IEmailRepository
    {
        IUnitOfWork UnitOfWork { get; }
        Email GetByKey(object keyValue);
        IQueryable<Email> GetQuery();
        void Add(Email entity);        void Remove(Email entity);
        void Attach(Email entity);
        IEnumerable<Email> GetAll();
        void Save(Email entity);
        void Update(Email entity, Email existingEntity);
        Email FindOne(Expression<Func<Email, bool>> criteria);
        IEnumerable<Email> FindList(Expression<Func<Email, bool>> criteria);
        void Dispose();
    }
}
