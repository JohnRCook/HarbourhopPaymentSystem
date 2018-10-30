using HarbourhopPaymentSystem.Data;
using HarbourhopPaymentSystem.Data.Repositories;
using HarbourhopPaymentSystem.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HarbourhopPaymentSystem
{
    public sealed class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json",
                     optional: false,
                     reloadOnChange: true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>();
            }

            _configuration = builder.Build();
        }


        private readonly IConfiguration _configuration;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureApplicationSettings(services);

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            ConfigureDatabase(services);

            services.AddScoped<PaymentService>();
            services.AddScoped<DanceCampService>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        private void ConfigureDatabase(IServiceCollection services)
        {
            services.AddDbContext<PaymentDatabaseContext>(
                options => options.UseSqlServer(_configuration.GetConnectionString("DefaultConnection"))
            );

            services.AddTransient<DatabaseInitializer>();
            services.AddScoped<BookingPaymentRepository>();
        }

        private void ConfigureApplicationSettings(IServiceCollection services)
        {
            // Required to use the Options<T> pattern
            services.AddOptions();

            services.Configure<MollieOptions>(_configuration.GetSection("MollieOptions"));
            services.Configure<DanceCampOptions>(_configuration.GetSection("DanceCampOptions"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
