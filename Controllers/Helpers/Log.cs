using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Web;

/// <summary>
/// Summary description for Log
/// </summary>
public class Log
{
    private static HttpApplication _app = null;
    private static HttpServerUtility Server
    {
        get
        {
            if (HttpContext.Current != null)
                return HttpContext.Current.Server;

            if (_app == null)
                _app = new HttpApplication();
            return _app.Server;
        }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public static void Debug(string msg)
    {

        string dir = HttpContext.Current.Server.MapPath("~/App_Data/Log/");
        Directory.CreateDirectory(dir);
        using (StreamWriter sw = File.AppendText(dir + "log.txt"))
        {
            sw.WriteLine(DateTime.Now + " " + msg);
        }

    }

}
