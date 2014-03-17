using Shnexy.DataAccessLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace Shnexy.DataAccessLayer
{
    //This generic repository ensures minimum repetition in all of the other repositories.
    //The database context is injected via a UnitOfWork implementation
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class GenericRepository<TEntity>
        where TEntity : class
    {
        private readonly IUnitOfWork _unitOfWork;
        internal DbSet<TEntity> dbSet;

        public GenericRepository(IUnitOfWork unitOfWork)
        {
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");
            _unitOfWork = unitOfWork;
            this.dbSet = _unitOfWork.Db.Set<TEntity>();
        }



        #region Property
        public IUnitOfWork UnitOfWork { get { return _unitOfWork; } }
        internal DbContext Database { get { return _unitOfWork.Db; } }

        #endregion

        #region Method

        public TEntity GetByKey(object keyValue)
        {
            return dbSet.Find(keyValue);
        }

        public IQueryable<TEntity> GetQuery()
        {
            return dbSet.AsEnumerable().AsQueryable<TEntity>();
        }


        public void Add(TEntity entity)
        {
            dbSet.Add(entity);
        }

        public void Remove(TEntity entity)
        {
            dbSet.Remove(entity);

        }

        public void Attach(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            dbSet.Attach(entity);
            _unitOfWork.Db.Entry(entity).State = EntityState.Modified;
        }

        public IEnumerable<TEntity> GetAll()
        {
            return dbSet.AsEnumerable().ToList();
        }

        public virtual void Save(TEntity entity)
        {

            //dbSet.Attach(entity);
            _unitOfWork.Db.Entry(entity).State = EntityState.Modified;

            //TEntity attachedEntity = dbSet.Local.SingleOrDefault(e => e.Id == entity.Id);  // You need to have access to key

            //if (attachedEntity != null)
            //{
            //    var attachedEntry = Database.Entry(attachedEntity);
            //    attachedEntry.CurrentValues.SetValues(entity);
            //}
            //else
            //{
            //    _unitOfWork.Db.Entry(entity).State = EntityState.Modified; // This should attach entity

            //}

        }
        //http://stackoverflow.com/a/12587752/1915866
        public virtual void Update(TEntity entity, TEntity existingEntity)
        {

            _unitOfWork.Db.Entry(existingEntity).CurrentValues.SetValues(entity);
            //dbSet.Attach(entity);
            //_unitOfWork.Db.Entry(entity).State = EntityState.Modified;

            //var set = Database.Set<TEntity>();
            //TEntity attachedEntity = dbSet.Local.SingleOrDefault(e => e.Id == entity.Id);  // You need to have access to key

            //if (attachedEntity != null)
            //{
            //    var attachedEntry = Database.Entry(attachedEntity);
            //    attachedEntry.CurrentValues.SetValues(entity);
            //}
            //else
            //{
            //    _unitOfWork.Db.Entry(entity).State = EntityState.Modified; // This should attach entity
            //}
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