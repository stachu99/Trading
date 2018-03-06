using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Hangfire;
using Hangfire.SQLite;
using System;
using Microsoft.AspNetCore.Mvc;
using YahooScraper.Services;
using Microsoft.AspNetCore.Localization;
using System.Collections.Generic;
using System.Globalization;

namespace YahooScraper
{
    public class Startup
    {
        public static IConfigurationRoot Configuration;
        private ILogger<Startup> _logger;
        public Startup(IHostingEnvironment env, ILogger<Startup> logger)
        {
            _logger = logger;
            var builder = new ConfigurationBuilder()
                             //.SetBasePath(env.ContentRootPath)
                             .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
                             .AddJsonFile($"appSettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                             .AddEnvironmentVariables();
            Configuration = builder.Build();
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //services.Configure<RequestLocalizationOptions>(options =>
            //{
            //    options.DefaultRequestCulture = new RequestCulture("en-US");
            //});
            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("en-GB");
                //By default the below will be set to whatever the server culture is. 
                options.SupportedCultures = new List<CultureInfo> { new CultureInfo("en-GB")};
                options.RequestCultureProviders = new List<IRequestCultureProvider>();
            });

            // HangFire configured to use a NLog database
            services.AddHangfire(x => x.UseSQLiteStorage(Configuration.GetValue<string>("NLog:NLogConnectionString")));

            // services.AddHangfire(x => x.UseSQLiteStorage("HangFireDB:HangFireConnectionString"));
            services.AddMvc()
                .AddMvcOptions(o => o.ReturnHttpNotAcceptable = true);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseRequestLocalization();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler();
            }

            app.UseMvc();

            try
            {
                var backgroundJobServerOptions = new BackgroundJobServerOptions
                {
                    WorkerCount = 1,
                };
                app.UseHangfireServer(backgroundJobServerOptions);
#if DEBUG
                RecurringJob.AddOrUpdate(() => IPOBackgroundService.RunAsync(), Cron.Minutely);
#else
                RecurringJob.AddOrUpdate(() => IPOBackgroundService.RunAsync(), Cron.Hourly);
#endif
            }
            catch (Exception e)
            {
                _logger.LogError(e, "HangFireService stopped program");
                throw;
            }

        }

    }
}
