using System;
using Data.DataAccessLayer.Infrastructure;
using Data.DataAccessLayer.Interfaces;
using Data.DataAccessLayer.Repositories;
using Data.DataAccessLayer.StructureMap;
using Data.Models;
using StructureMap;

namespace Playground
{
    class Program
    {
        /// <summary>
        /// This is a sandbox for devs to use. Useful for directly calling some library without needing to launch the main application
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            StructureMapBootStrapper.ConfigureDependencies(String.Empty);
            var con = new ShnexyDbContext();
            con.Database.Initialize(true);


            var uow = ObjectFactory.GetInstance<IUnitOfWork>();
            var attachmentRepo = new AttachmentRepository(uow);
            
            //var attachment = new Attachment
            //{
            //    OriginalName = "My Attachment",
            //    StringData = "Testing123"
            //};
            //attachmentRepo.Add(attachment);


            var sfRepo = new StoredFileRepository(uow);
            var sf = new StoredFile()
            {
                OriginalName = "My File",
                StringData = "Testing123"
            };
            sfRepo.Add(sf);
            attachmentRepo.UnitOfWork.SaveChanges();

        }
    }
}
