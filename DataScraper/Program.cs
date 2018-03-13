using System;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using NLog.Web;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using NLog.LayoutRenderers;

namespace DataScraper
{
    public class Program
    {
        private static IConfigurationRoot configuration;
        public static void Main(string[] args)
        {
            var h = new WebHostBuilder();
            var environment = h.GetSetting("environment");
            var builder = new ConfigurationBuilder()
                    .AddJsonFile("appSettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"appSettings.{environment}.json", optional: true)
                    .AddEnvironmentVariables();
            configuration = builder.Build();
            LayoutRenderer.Register("NLogConnectionString", (logEvent) => configuration.GetValue<string>("NLog:NLogConnectionString"));
            LayoutRenderer.Register("NlogDB", (logEvent) => configuration.GetValue<string>("NLog:NLogDB"));
            // NLog: Database for logging
            EnsureDB();
            // NLog: setup the logger first to catch all errors
            //var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger(); - OBSOLETE
            var logger = NLog.LogManager.LoadConfiguration("nlog.config").GetCurrentClassLogger();

            try
            {
                logger.Debug("init main");
                BuildWebHost(args).Run();
            }
            catch (Exception e)
            {
                //NLog: catch setup errors
                logger.Error(e, "Stopped program because of exception");
                throw;
            }
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseNLog()  // NLog: setup NLog for Dependency injection
                .Build();

        //NLog: Database for logging
        static void EnsureDB()
        {
            if (File.Exists(configuration.GetValue<string>("NLog:NLogDB")))
            {
                return;
            }
            using (SqliteConnection connection = new SqliteConnection(configuration.GetValue<string>("NLog:NLogConnectionString")))
            using (SqliteCommand command = new SqliteCommand(@"CREATE TABLE `NLog` (
	`Id`	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
	`Application`	TEXT NOT NULL,
	`Logged`	TEXT NOT NULL,
	`Level`	TEXT NOT NULL,
	`Message`	TEXT NOT NULL,
	`UserName`	TEXT,
	`ServerName`	TEXT,
	`Port`	TEXT,
	`Url`	TEXT,
	`Https`	INTEGER,
	`ServerAddress`	TEXT,
	`RemoteAddress`	TEXT,
	`Logger`	TEXT,
	`CallSite`	TEXT,
	`Exception`	TEXT
); ", connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}
