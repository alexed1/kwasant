using Data.Entities;
using Data.Interfaces;
namespace Data.Repositories
{
   public class IncidentRepository : GenericRepository<IncidentDO>, IIncidentRepository
    {
       internal IncidentRepository(IUnitOfWork uow)
           : base(uow)
       {

       }

       public override void Add(IncidentDO entity)
        {
            base.Add(entity);
        }

       
    }
   public interface IIncidentRepository : IGenericRepository<IncidentDO>
   {
   }

    
   
}
