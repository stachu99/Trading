﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Hangfire;
using Hangfire.SQLite;
using System;

namespace YahooIPOScraper
{
    public class Startup
    {
        public static IConfigurationRoot Configuration;
        private ILogger<Startup> _logger;
        public Startup(IHostingEnvironment env, ILogger<Startup> logger)
        {
            _logger = logger;
            var builder = new ConfigurationBuilder()
     .SetBasePath(env.ContentRootPath)
     .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
     .AddJsonFile($"appSettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
     .AddEnvironmentVariables();

            Configuration = builder.Build();

        }


        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

       services.AddHangfire(x => x.UseSQLiteStorage(Configuration.GetValue<string>("HangFireDB:HangFireConnectionString")));

           // services.AddHangfire(x => x.UseSQLiteStorage("HangFireDB:HangFireConnectionString"));
            services.AddMvc()
    .AddMvcOptions(o => o.ReturnHttpNotAcceptable = true);
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
                app.UseExceptionHandler();
            }
            app.UseHangfireServer();
            //BackgroundJob.Schedule(() => DoBacground(Message), TimeSpan.FromSeconds(5));
            //BackgroundJob.Enqueue(() => DoBacground(Message));
            app.UseMvc();


            //app.Run(async (context) =>
            //{
            //    await context.Response.WriteAsync(Message);

            //});

        }
    }
}
