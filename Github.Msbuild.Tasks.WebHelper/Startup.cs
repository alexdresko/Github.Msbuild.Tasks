using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Github.Msbuild.Tasks.WebHelper.Startup))]
namespace Github.Msbuild.Tasks.WebHelper
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
