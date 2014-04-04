using System;
using System.Configuration;
using Configuration;
using Daemons;
using Shnexy;
using Shnexy.DataAccessLayer.StructureMap;

namespace Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            StructureMapBootStrapper.ConfigureDependencies(String.Empty);
            var daemonConfig = ConfigurationManager.GetSection("daemonSettings") as DaemonSettings;
            if (daemonConfig != null)
            {
                if (daemonConfig.Enabled)
                {
                    foreach (DaemonConfig daemon in daemonConfig.Daemons)
                    {
                        if (daemon.Enabled)
                        {
                            var type = Type.GetType(daemon.InitClass, true);
                            var obj = Activator.CreateInstance(type) as IDaemon;
                            if (obj == null)
                                throw new ArgumentException(string.Format("An daemon must implement IDaemon. Type '{0}' does not implement the interface.", type.Name));
                            obj.Start();
                        }
                    }
                }
            }
        }
    }

}
