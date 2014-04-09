﻿using System;
using System.Data.Entity;

namespace Data.DataAccessLayer.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        #region Method

        /// <summary>
        /// Call this to commit the unit of work
        /// </summary>
        void Commit();

        /// <summary>
        /// Return the database reference for this UOW
        /// </summary>
        DbContext Db { get; }

        /// <summary>
        /// Starts a transaction on this unit of work
        /// </summary>
        void StartTransaction();

        /// <summary>
        /// The save changes.
        /// </summary>
        void SaveChanges();

        /// <summary>
        /// The save changes.
        /// </summary>
        // void SaveChanges(SaveOptions saveOptions);
        #endregion
    }
}
