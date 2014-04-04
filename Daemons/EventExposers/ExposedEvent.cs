using System;
using System.Reflection;

namespace Daemons.EventExposers
{
    public abstract class ExposedEvent
    {
        private readonly String m_Name;
        private readonly Type m_Owner;

        protected ExposedEvent(string name, Type ownerType)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (ownerType == null) throw new ArgumentNullException("ownerType");

            m_Name = name;
            m_Owner = ownerType;
        }

        public static implicit operator EventInfo(ExposedEvent exposedEvent)
        {
            return exposedEvent.m_Owner.GetEvent(exposedEvent.m_Name);
        }
    }
    public abstract class ExposedEvent<T> : ExposedEvent
    {
        protected ExposedEvent(string name)
            : base(name, typeof(T))
        {
        }
    }
}
