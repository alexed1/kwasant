using System;
using System.Collections.Generic;

namespace KwasantCore.ExternalServices
{
    public static class ServiceManager
    {
        public static Dictionary<Type, ServiceInformation> ServiceInfo = new Dictionary<Type, ServiceInformation>();

        public static ServiceInformation GetInformationForService<TServiceType>()
        {
            return GetInformationForService(typeof (TServiceType));
        }
        public static ServiceInformation GetInformationForService(Type serviceType)
        {
            lock (ServiceInfo)
                return ServiceInfo[serviceType];
        }

        public static void RegisterService<T>(String serviceName)
        {
            lock (ServiceInfo)
            {
                ServiceInformation service;
                if (ServiceInfo.ContainsKey(typeof (T)))
                    service = ServiceInfo[typeof (T)];
                else
                    ServiceInfo[typeof(T)] = service = new ServiceInformation();
                
                service.ServiceName = serviceName;
            }
                
        }

        public static void LogEvent<T>(String eventName)
        {
            lock (ServiceInfo)
                ServiceInfo[typeof(T)].AddEvent(eventName);
        }

        public class ServiceInformation
        {
            private String _serviceName;
            public String ServiceName
            {
                get
                {
                    lock (ServiceInfo)
                        return _serviceName;
                }
                set
                {
                    lock (ServiceInfo)
                        _serviceName = value;
                }
            }

            private readonly List<String> _events = new List<string>();
            public List<String> Events
            {
                get
                {
                    lock (ServiceInfo)
                        return new List<string>(_events);
                }
            }

            public void AddEvent(String eventName)
            {
                lock (ServiceInfo)
                    _events.Add(eventName);
            }
        }
    }

    public class ServiceManager<T>
    {
        public ServiceManager(String serviceName)
        {
            ServiceManager.RegisterService<T>(serviceName);
        }

        public void LogEvent(String eventName)
        {
            ServiceManager.LogEvent<T>(eventName);
        }
    }
}
