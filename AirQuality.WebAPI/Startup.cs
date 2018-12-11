using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AirQualityWebAPI.Repository;
using AirQualityWebAPI.HubSignalR;

namespace AirQualityWebAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Air quality repository dependency resolver
            services.AddSingleton<IAirQualityMeasurementRepository, AirQualityMeasurementRepository>();
            services.AddSingleton(provider => Configuration);
            services.AddMvc();
            services.AddSignalR().AddAzureSignalR();

        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAzureSignalR(routes =>
            {
                routes.MapHub<LogPointHubMessage>("/livedatastream");
            });

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseMvc();
        }
    }
}
