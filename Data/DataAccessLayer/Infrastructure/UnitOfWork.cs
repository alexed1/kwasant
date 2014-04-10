using System;
using System.Data.Entity;
using System.Transactions;
using Data.DataAccessLayer.Interfaces;

namespace Data.DataAccessLayer.Infrastructure
{
     public class UnitOfWork : IUnitOfWork
    {
        private TransactionScope transaction;
        public DbContext db;


        public UnitOfWork(DbContext curDbContext)
        {
            db = curDbContext;
        }

        public void Save()
        {
            db.SaveChanges();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (transaction != null)
                transaction.Dispose();
        }

        public void StartTransaction()
        {
            transaction = new TransactionScope();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public void Commit()
        {
            db.SaveChanges();
            transaction.Complete();
            transaction.Dispose();
        }
        public void SaveChanges()
        {
            db.SaveChanges();
        }

        public DbContext Db
        {
            get { return db; }
        }

    }
}
