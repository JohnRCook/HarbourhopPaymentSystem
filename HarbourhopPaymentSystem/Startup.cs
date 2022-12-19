using HarbourhopPaymentSystem.Data;
using HarbourhopPaymentSystem.Data.Repositories;
using HarbourhopPaymentSystem.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace HarbourhopPaymentSystem
{
    public sealed class Startup
    {
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        private readonly IConfiguration _configuration;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureApplicationSettings(services);

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = _ => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            ConfigureDatabase(services);

            services.AddScoped<PaymentService>();
            services.AddScoped<DanceCampService>();

            services.AddMvc();
        }

        private void ConfigureDatabase(IServiceCollection services)
        {
            services.AddDbContext<PaymentDatabaseContext>(
                options => options.UseSqlServer(_configuration.GetConnectionString("DefaultConnection"))
            );

            services.AddTransient<DatabaseInitializer>();
            services.AddScoped<BookingPaymentRepository>();
            services.AddSingleton(ConfigureLogger());
        }

        private ILogger ConfigureLogger()
        {
            return new LoggerConfiguration()
                   .ReadFrom.Configuration(_configuration)
                   .CreateLogger();
        }

        private void ConfigureApplicationSettings(IServiceCollection services)
        {
            // Required to use the Options<T> pattern
            services.AddOptions();

            services.Configure<MollieOptions>(_configuration.GetSection("MollieOptions"));
            services.Configure<DanceCampOptions>(_configuration.GetSection("DanceCampOptions"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Register before static files to set the response headers for each request.
            app.UseHsts();
            
            app.UseExceptionHandler("/Home/Error");

            //app.UseHttpsRedirection();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseRouting();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
