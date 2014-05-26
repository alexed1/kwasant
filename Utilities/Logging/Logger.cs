using System;
using System.Diagnostics;
using log4net;

namespace Utilities.Logging
{
    public static class Logger
    {
        static Logger()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        public static ILog GetLogger()
        {
            return LogManager.GetLogger(new StackTrace().GetFrame(1).GetMethod().DeclaringType);
        }
    }

    


}
