using Data.Constants;
using Data.Interfaces;
using StructureMap;

namespace Data.Infrastructure
{
    public class ShnexyInitializer : System.Data.Entity.DropCreateDatabaseIfModelChanges<ShnexyDbContext>
    {
        protected override void Seed(ShnexyDbContext context)
        {
            IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>();
            EmailStatusConstants.ApplySeedData(uow);
            InstructionConstants.ApplySeedData(uow);
            uow.SaveChanges();
        }
    }
}