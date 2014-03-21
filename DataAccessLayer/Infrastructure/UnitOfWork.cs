using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Shnexy.Models;
using Shnexy.DataAccessLayer.Interfaces;
using System.Data.Entity;
using System.Transactions;
using System.Data.Entity.Core;

namespace Shnexy.DataAccessLayer
{
     public class UnitOfWork : IUnitOfWork
    {
        private TransactionScope transaction;
        public ShnexyDbContext db;


        public UnitOfWork(ShnexyDbContext curDbContext)
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
