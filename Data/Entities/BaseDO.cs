using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Data.Infrastructure;
using Data.Interfaces;
using Data.States.Templates;
using Utilities;

namespace Data.Entities
{
    public class BaseDO : IBaseDO, IModifyHook
    {
        public DateTimeOffset LastUpdated { get; set; }

        public void OnModify(DbPropertyValues originalValues, DbPropertyValues currentValues)
        {
            var idProperty = ReflectionHelper.EntityPrimaryKeyPropertyInfo(this);
            var id = idProperty.GetValue(this);
            var stateProperties = GetType().GetProperties()
                .Where(p => p.PropertyType.GetInterfaces().Any(i => i == typeof (IStateTemplate<>)))
                .ToArray();
            foreach (var stateProperty in stateProperties)
            {
                if (!MiscUtils.AreEqual(originalValues[stateProperty.Name], currentValues[stateProperty.Name]))
                {
                    // AlertManager.TrackablePropertyUpdated("StateChange", "BookingRequest", id, uow.BookingRequestStatusRepository.GetByKey(State).Name);
                }
            }
        }
    }
}
