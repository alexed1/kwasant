using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Data.Entities;

namespace Data.Interfaces
{
    public interface IDBContext : IDisposable
    {
        int SaveChanges();

        IDbSet<TEntity> Set<TEntity>()
            where TEntity : class;

        DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity)
            where TEntity : class;

        IUnitOfWork UnitOfWork { get; set; }
    }
}
