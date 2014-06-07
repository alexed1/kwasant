using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Data.Interfaces;

namespace Data.Repositories
{
    //This generic repository ensures minimum repetition in all of the other repositories.
    //The database context is injected via a UnitOfWork implementation
    /// <typeparam name="TEntity"></typeparam>
    public class GenericRepository<TEntity> : IDisposable, IGenericRepository<TEntity> where TEntity : class
    {
        protected readonly IDBContext DBContext;
        public IDbSet<TEntity> DBSet;

        internal GenericRepository(IDBContext dbContext)
        {
            if (dbContext == null) throw new ArgumentNullException("dbContext");
            DBContext = dbContext;
            DBSet = dbContext.Set<TEntity>();
        }

        #region Property
        public IUnitOfWork UnitOfWork { get { return DBContext.UnitOfWork; } }

        #endregion

        #region Method

        public TEntity GetByKey(object keyValue)
        {
            return DBSet.Find(keyValue);
        }

        public IQueryable<TEntity> GetQuery()
        {
            return DBSet.AsQueryable();
        }

        public void Attach(TEntity entity)
        {
            DBSet.Attach(entity);
        }

        public virtual void Add(TEntity entity)
        {
            DBSet.Add(entity);
        }

        public void Remove(TEntity entity)
        {
            DBSet.Remove(entity);
        }

        public IEnumerable<TEntity> GetAll()
        {
            
            return DBSet.AsEnumerable().ToList();
        }

        public TEntity FindOne(Expression<Func<TEntity, bool>> criteria)
        {
            return DBSet.Where(criteria).FirstOrDefault();
        }

        public IEnumerable<TEntity> FindList(Expression<Func<TEntity, bool>> criteria)
        {
            return GetQuery().Where(criteria).ToList();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            DBContext.UnitOfWork.Dispose();
        }

        #endregion



    }
}