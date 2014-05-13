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
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        private readonly IUnitOfWork _unitOfWork;
        public IDbSet<TEntity> dbSet;

        public GenericRepository(IUnitOfWork unitOfWork)
        {
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");
            _unitOfWork = unitOfWork;
            this.dbSet = _unitOfWork.Db.Set<TEntity>();
        }



        #region Property
        public IUnitOfWork UnitOfWork { get { return _unitOfWork; } }
        internal IDBContext Database { get { return _unitOfWork.Db; } }

        #endregion

        #region Method

        public TEntity GetByKey(object keyValue)
        {
            return dbSet.Find(keyValue);
        }

        public IQueryable<TEntity> GetQuery()
        {
            return dbSet.AsEnumerable().AsQueryable();
        }


        public void Add(TEntity entity)
        {
            dbSet.Add(entity);
        }

        public void Remove(TEntity entity)
        {
            dbSet.Remove(entity);
        }

        public IEnumerable<TEntity> GetAll()
        {
            
            return dbSet.AsEnumerable().ToList();
        }

        public TEntity FindOne(Expression<Func<TEntity, bool>> criteria)
        {
            return dbSet.Where(criteria).FirstOrDefault();
        }

        public IEnumerable<TEntity> FindList(Expression<Func<TEntity, bool>> criteria)
        {
            return GetQuery().Where(criteria).ToList();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        #endregion



    }
}