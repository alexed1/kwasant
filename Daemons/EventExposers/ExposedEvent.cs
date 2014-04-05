using System;
using System.Reflection;

namespace Daemons.EventExposers
{
    public abstract class ExposedEvent
    {
        private readonly String _name;
        private readonly Type _owner;

        protected ExposedEvent(string name, Type ownerType)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (ownerType == null) throw new ArgumentNullException("ownerType");

            _name = name;
            _owner = ownerType;
        }

        public static implicit operator EventInfo(ExposedEvent exposedEvent)
        {
            return exposedEvent._owner.GetEvent(exposedEvent._name);
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
