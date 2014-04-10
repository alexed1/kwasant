using System;
using System.Configuration;
using Configuration;
using Daemons;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Shnexy.Startup))]
namespace Shnexy
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureDaemons();

            //ConfigureAuth(app);
        }

        private static void ConfigureDaemons()
        {
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
                            var obj = Activator.CreateInstance(type) as Daemon;
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
