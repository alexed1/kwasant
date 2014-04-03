using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Shnexy.Startup))]
namespace Shnexy
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //ConfigureAuth(app);
        }
    }
}
