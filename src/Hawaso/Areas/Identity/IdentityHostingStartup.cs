using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(Hawaso.Areas.Identity.IdentityHostingStartup))]
namespace Hawaso.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
            });
        }
    }
}