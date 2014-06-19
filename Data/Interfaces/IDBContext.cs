using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Data.Infrastructure;

namespace Data.Interfaces
{
    public interface IDBContext : IDisposable
    {
        int SaveChanges();

        List<KwasantDbContext.PropertyChangeInformation> GetEntityModifications<T>(T entity)
            where T : class;

        List<KwasantDbContext.EntityChangeInformation> GetModifiedEntities();

        IDbSet<TEntity> Set<TEntity>()
            where TEntity : class;

        DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity)
            where TEntity : class;
    }
}
