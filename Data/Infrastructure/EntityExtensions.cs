using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces;
using Data.States.Templates;
using Utilities;

namespace Data.Infrastructure
{
    static class EntityExtensions
    {
        public static void DetectStateUpdates(this IBaseDO entity,
            DbPropertyValues originalValues, DbPropertyValues currentValues)
        {
            var stateProperties = entity.GetType().GetProperties()
                .Where(p => p.PropertyType.GetInterfaces().Any(i => i == typeof (IStateTemplate<>)))
                .ToArray();
            entity.DetectUpdates(originalValues, currentValues, stateProperties,
                stateProperty => stateProperty.PropertyType.GetInterfaces().First(t => t == typeof(IStateTemplate<>)).GetGenericArguments()[0].Name,
                stateTemplate => stateTemplate != null ? ((IStateTemplate)stateTemplate).Name : null);
        }

        public static void DetectUpdates(this IBaseDO entity, 
            DbPropertyValues originalValues, DbPropertyValues currentValues, PropertyInfo[] properties,
            Func<PropertyInfo, string> propertyNameFunc = null, Func<object, object> valueFunc = null)
        {
            if (propertyNameFunc == null)
                propertyNameFunc = info => info.Name;
            if (valueFunc == null)
                valueFunc = o => o;
            var idProperty = ReflectionHelper.EntityPrimaryKeyPropertyInfo(entity);
            var id = idProperty.GetValue(entity);
            var type = entity.GetType();
            foreach (var stateProperty in properties)
            {
                if (!MiscUtils.AreEqual(originalValues[stateProperty.Name], currentValues[stateProperty.Name]))
                {
                    var entityName = type.Name;
                    if (entityName.EndsWith("DO", StringComparison.Ordinal))
                        entityName = entityName.Remove(entityName.Length - 2);
                    AlertManager.TrackablePropertyUpdated(entityName, propertyNameFunc(stateProperty), id, valueFunc(currentValues[stateProperty.Name]));
                }
            }
        }
    }
}
