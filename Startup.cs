using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using Configuration;
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
                    /* When we extract the data layer from Shnexy, we can fix up our dependencies, so that we no longer have to load the assembly at run time.
                     * When this is fixed, please also move IDaemon into the Daemon project */
                    try
                    {
                        Assembly.Load(daemonConfig.DaemonAssemblyName);
                    }
                    catch (FileNotFoundException)
                    {
                        throw new Exception(
                            "Daemons project .dlls are missing from the bin folder, or the assembly is incorrectly named in web.config.");
                    }

                    foreach (DaemonConfig daemon in daemonConfig.Daemons)
                    {
                        if (daemon.Enabled)
                        {
                            var type = Type.GetType(daemon.InitClass, true);
                            var obj = Activator.CreateInstance(type) as IDaemon;
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
