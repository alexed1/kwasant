using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Data.Interfaces
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        IUnitOfWork UnitOfWork { get; }
        TEntity GetByKey(object keyValue);
        IQueryable<TEntity> GetQuery();
        void Add(TEntity entity);
        void Remove(TEntity entity);
        IEnumerable<TEntity> GetAll();        
        TEntity FindOne(Expression<Func<TEntity, bool>> criteria);
        IEnumerable<TEntity> FindList(Expression<Func<TEntity, bool>> criteria);
        void Dispose();
    }
}