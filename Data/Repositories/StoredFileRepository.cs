﻿using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class StoredFileRepository : GenericRepository<StoredFileDO>, IStoredFileRepository
    {

        public StoredFileRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }


    public interface IStoredFileRepository : IGenericRepository<StoredFileDO>
    {

    }
}
