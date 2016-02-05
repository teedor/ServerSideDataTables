using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SkippyWeb.Startup))]
namespace SkippyWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
