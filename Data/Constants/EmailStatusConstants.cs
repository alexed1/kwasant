using System.Collections.Generic;
using System.Linq;
using Data.DataAccessLayer.Interfaces;
using Data.DataAccessLayer.Repositories;
using Data.Models;
using StructureMap;

namespace Data.Constants
{
    public class EmailStatusConstants
    {
        public const int QUEUED = 1;
        public const int SENT = 2;

        private static readonly object CachedStatusLocker = new object();
        private static IDictionary<int, EmailStatus> _cachedStatuses;
        public static EmailStatus GetStatusRow(int statusID)
        {
            lock (CachedStatusLocker)
            {
                if (_cachedStatuses != null)
                    return _cachedStatuses[statusID];

                _cachedStatuses = new Dictionary<int, EmailStatus>();
                var uow = ObjectFactory.GetInstance<IUnitOfWork>();
                var emailStatusRepo = new EmailStatusRepository(uow);
                var statuses = emailStatusRepo.GetAll();
                _cachedStatuses = statuses.ToDictionary(s => s.EmailStatusID, s => s);
                return _cachedStatuses[statusID];
            }
        }

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
