using Data.Entities;
using Data.Interfaces;
using Data.Validators;
using FluentValidation;

namespace Data.Repositories
{
    public class EnvelopeRepository : GenericRepository<EnvelopeDO>, IEnvelopeRepository
    {
        private readonly EnvelopeValidator _validator;

        internal EnvelopeRepository(IUnitOfWork uow) : base(uow)
        {
            _validator = new EnvelopeValidator();
        }

        public override void Add(EnvelopeDO entity)
        {
            _validator.ValidateAndThrow(entity);
            base.Add(entity);
        }
    }

    public interface IEnvelopeRepository : IGenericRepository<EnvelopeDO>
    {
        
    }

}
