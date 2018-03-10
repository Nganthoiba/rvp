using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(RVP.Startup))]
namespace RVP
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
