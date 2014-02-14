using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Shnexy.Models;

namespace Shnexy.DataAccessLayer
{
    public class UnitOfWork : IDisposable
    {
        private ShnexyDbContext context = new ShnexyDbContext();
        private GenericRepository<Queue> queueRepository;
        private GenericRepository<Message> messageRepository;

        public GenericRepository<Message> DepartmentRepository
        {
            get
            {

                if (this.messageRepository == null)
                {
                    this.messageRepository = new GenericRepository<Message>(context);
                }
                return messageRepository;
            }
        }

        public GenericRepository<Queue> QueueRepository
        {
            get
            {

                if (this.queueRepository == null)
                {
                    this.queueRepository = new GenericRepository<Queue>(context);
                }
                return queueRepository;
            }
        }

        public void Save()
        {
            context.SaveChanges();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}