using System.Diagnostics;

namespace Data.DataAccessLayer.Infrastructure
{
    public class ShnexyInitializer : System.Data.Entity.DropCreateDatabaseIfModelChanges<ShnexyDbContext>
    {

        protected override void Seed(ShnexyDbContext context)
        {
            
            Debug.WriteLine("in seed");
        }
    }
}