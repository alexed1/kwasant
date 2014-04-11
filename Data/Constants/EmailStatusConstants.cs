using Data.DataAccessLayer.Interfaces;
using Data.DataAccessLayer.Repositories;
using Data.Models;

namespace Data.Constants
{
    public class EmailStatusConstants
    {
        public const int QUEUED = 1;
        public const int SENT = 2;
        public const int UNPROCESSED = 3;
        
        public static void ApplySeedData(IUnitOfWork uow)
        {
            var constants = typeof (EmailStatusConstants).GetFields();
            var emailStatusRepo = new EmailStatusRepository(uow);
            foreach (var constant in constants)
            {
                var name = constant.Name;
                var value = constant.GetValue(null);
                emailStatusRepo.Add(new EmailStatus
                {
                    EmailStatusID = (int)value,
                    Value = name
                });
            }
        }
    }
}
