using System;
using System.Configuration;
using Configuration;
using Daemons;
using KwasantWeb;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace KwasantWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureDaemons();
        }

        private static void ConfigureDaemons()
        {
            DaemonSettings daemonConfig = ConfigurationManager.GetSection("daemonSettings") as DaemonSettings;
            if (daemonConfig != null)
            {
                if (daemonConfig.Enabled)
                {
                    foreach (DaemonConfig daemon in daemonConfig.Daemons)
                    {
                        if (daemon.Enabled)
                        {
                            Type type = Type.GetType(daemon.InitClass, true);
                            Daemon obj = Activator.CreateInstance(type) as Daemon;
                            if (obj == null)
                                throw new ArgumentException(
                                    string.Format(
                                        "An daemon must implement IDaemon. Type '{0}' does not implement the interface.",
                                        type.Name));
                            obj.Start();
                        }
                    }
                }
            }
        }
    }
}
