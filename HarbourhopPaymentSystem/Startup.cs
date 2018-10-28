using System;
using HarbourhopPaymentSystem.Data;
using HarbourhopPaymentSystem.Data.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mollie.Api.Client;
using Mollie.Api.Client.Abstract;

namespace HarbourhopPaymentSystem
{
    public sealed class Startup
    {
        private Settings _settings;

        public Startup()
        {
            _configuration = GetConfiguration();
        }

        private IConfiguration GetConfiguration()
        {
            var builder = new ConfigurationBuilder();
            IConfigurationRoot configuration =
                builder.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                       .AddJsonFile("appsettings.json")
                       .AddUserSecrets<Settings>()
                       .Build();

            return configuration;
        }

        private readonly IConfiguration _configuration;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureSettings();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            ConfigureDatabase(services);

            services.AddSingleton(_settings);
            services.AddScoped<IPaymentClient>(_ => new PaymentClient(_settings.MollieApiKey));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        private void ConfigureDatabase(IServiceCollection services)
        {
            services.AddDbContext<PaymentDatabaseContext>(
                options => options.UseSqlServer(_settings.DefaultConnection)
            );

            services.AddTransient<DatabaseInitializer>();
            services.AddScoped<BookingPaymentRepository>();
        }

        private void ConfigureSettings()
        {
            var configurationSection = _configuration.GetSection("AppSettings");
            _settings = new Settings();
            configurationSection.Bind(_settings);
            var connectionStringSection = _configuration.GetSection("ConnectionStrings");
            connectionStringSection.Bind(_settings);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, DatabaseInitializer databaseInitializer)
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

            databaseInitializer.Initialize();

            app.UseHttpsRedirection();
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
