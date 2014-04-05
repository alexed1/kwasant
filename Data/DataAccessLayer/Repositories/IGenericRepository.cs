using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Shnexy.DataAccessLayer.Interfaces;

namespace Shnexy.DataAccessLayer
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        IUnitOfWork UnitOfWork { get; }
        TEntity GetByKey(object keyValue);
        IQueryable<TEntity> GetQuery();
        void Add(TEntity entity);
        void Remove(TEntity entity);
        void Attach(TEntity entity);
        IEnumerable<TEntity> GetAll();
        void Save(TEntity entity);
        void Update(TEntity entity, TEntity existingEntity);
        TEntity FindOne(Expression<Func<TEntity, bool>> criteria);
        IEnumerable<TEntity> FindList(Expression<Func<TEntity, bool>> criteria);
        void Dispose();
    }
}