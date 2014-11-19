﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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
        private static Type GetGenericInterface(Type type, Type interfaceType)
        {
            return type.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i == interfaceType.MakeGenericType(i.GenericTypeArguments[0]));
        }

        public static IEnumerable<PropertyInfo> GetStateProperties(this IBaseDO entity)
        {
            foreach (var p in entity.GetType().GetProperties())
            {
                var navigationProperty = ReflectionHelper.ForeignKeyNavitationProperty(entity, p);
                if (navigationProperty != null && GetGenericInterface(navigationProperty.PropertyType, typeof(IStateTemplate<>)) != null)
                    yield return p;
            }
        }

        /// <summary>
        /// Detects any updates on specified entity state properties.
        /// </summary>
        /// <param name="entity">entity</param>
        /// <param name="originalValues">original properties values</param>
        /// <param name="currentValues">current properties values</param>
        public static void DetectStateUpdates(this IBaseDO entity,
            DbPropertyValues originalValues, DbPropertyValues currentValues)
        {
            var stateProperties = entity.GetStateProperties().ToArray();
            if (stateProperties.Length > 0)
            {
                entity.DetectUpdates(originalValues, currentValues, stateProperties,
                    stateProperty => GetGenericInterface(ReflectionHelper.ForeignKeyNavitationProperty(entity, stateProperty).PropertyType, typeof(IStateTemplate<>)).GetGenericArguments()[0].Name,
                    (stateProperty, stateKey) => stateKey != null 
                        ? GetGenericInterface(ReflectionHelper.ForeignKeyNavitationProperty(entity, stateProperty).PropertyType, typeof(IStateTemplate<>)).GetGenericArguments()[0].GetFields().Single(f => Equals(f.GetValue(entity), stateKey)).Name
                        : null);
            }
        }

        /// <summary>
        /// Detects any updates on specified entity and tracks them via AlertManager.TrackablePropertyUpdated alert.
        /// </summary>
        /// <param name="entity">entity</param>
        /// <param name="originalValues">original properties values</param>
        /// <param name="currentValues">current properties values</param>
        /// <param name="properties">properties to track</param>
        /// <param name="propertyNameFunc">optional function to manage property name for tracking. If null it takes PropertyInfo.Name value</param>
        /// <param name="valueFunc">optional function to manage property value for tracking. If null it takes currentValues[propertyName] value</param>
        /// <remarks>
        /// This method would likely be called from <see cref="IModifyHook.OnModify">OnModify</see> implementation to track properties. 
        /// For tracking enum typed (state) properties see <see cref="DetectStateUpdates">DetectStateUpdates</see> method.
        /// </remarks>
        public static void DetectUpdates(this IBaseDO entity, 
            DbPropertyValues originalValues, DbPropertyValues currentValues, PropertyInfo[] properties,
            Func<PropertyInfo, string> propertyNameFunc = null, Func<PropertyInfo, object, object> valueFunc = null)
        {
            if (properties == null || properties.Length == 0)
                return;
            if (propertyNameFunc == null)
                propertyNameFunc = info => info.Name;
            if (valueFunc == null)
                valueFunc = (p, o) => o;
            var type = entity.GetType();
            var idProperty = type.GetProperty("Id");
            var id = idProperty != null ? idProperty.GetValue(entity) : null;
            foreach (var property in properties)
            {
                if (!MiscUtils.AreEqual(originalValues[property.Name], currentValues[property.Name]))
                {
                    string entityName;
                    if (type.Name.EndsWith("DO", StringComparison.Ordinal))
                        entityName = type.Name.Remove(type.Name.Length - 2);
                    else if (type.BaseType != null && type.BaseType.Name.EndsWith("DO", StringComparison.Ordinal))
                        entityName = type.BaseType.Name.Remove(type.BaseType.Name.Length - 2);
                    else
                        entityName = type.Name;
                    AlertManager.TrackablePropertyUpdated(entityName, propertyNameFunc(property), id, valueFunc(property, currentValues[property.Name]));
                }
            }
        }
    }
}