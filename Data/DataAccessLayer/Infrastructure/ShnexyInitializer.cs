using Data.Constants;
using Data.DataAccessLayer.Interfaces;
using StructureMap;

namespace Data.DataAccessLayer.Infrastructure
{
    public class ShnexyInitializer : System.Data.Entity.DropCreateDatabaseIfModelChanges<ShnexyDbContext>
    {

        protected override void Seed(ShnexyDbContext context)
        {
            var uow = ObjectFactory.GetInstance<IUnitOfWork>();
            EmailStatusConstants.ApplySeedData(uow);
            uow.SaveChanges();
        }
    }
}