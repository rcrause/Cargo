using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CargoExample.Startup))]
namespace CargoExample
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureCargo(app);
            ConfigureAuth(app);
        }
    }
}
