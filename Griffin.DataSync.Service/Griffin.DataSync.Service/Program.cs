using Griffin.DataSync.Service.Configuration;
using Griffin.DataSync.Service.Helpers;
using Griffin.DataSync.Service.Infrastructure.ConnectionFactories;
using Griffin.DataSync.Service.Interfaces;
using Griffin.DataSync.Service.Jobs;
using Griffin.DataSync.Service.Repositories;
using Griffin.DataSync.Service.Services;
using Serilog;

namespace Griffin.DataSync.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var s = @"Provider=MSDASQL;Password=RPA4AAG;Persist Security Info=True;User ID=griffin;Extended Properties=""DSN=SOTAMAS90; UID=griffin; PWD=RPA4AAG; Directory=\\md-sage\Sage\Sage 100 Advanced\MAS90; ...; SERVER=NotTheServer""";
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(s));
            var builder = Host.CreateApplicationBuilder(args);
           // builder.Services.AddWindowsService();

            builder.Services.Configure<SyncOptions>(
                builder.Configuration.GetSection("Sync"));

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(
                     @"C:\Logs\GriffinDataSync\log-.txt",
                    rollingInterval: RollingInterval.Day)
                .CreateLogger();
            builder.Services.AddSerilog();
            builder.Services.AddSingleton<ISqlQueryProvider, SqlQueryProvider>();
            builder.Services.AddSingleton<IOdbcConnectionFactory,OdbcConnectionFactory>();
            builder.Services.AddSingleton<ISyncJob,InventoryReplenishmentSyncJob>();
            builder.Services.AddSingleton<ISqlConnectionFactory,SqlConnectionFactory>();
            builder.Services.AddSingleton<ISageRepository, SageRepository>();
            builder.Services.AddSingleton<ISyncJob, CIItemSyncJob>();
            builder.Services.AddSingleton<SageConnectorRunner>();
            builder.Services.AddSingleton<ISqlRepo, SqlRepo>();
            builder.Services.AddSingleton<ISyncEngine, SyncEngine>();
            builder.Services.AddSingleton<RetryService>();
            builder.Services.AddSingleton<ISyncJob, MBBinLocationSyncJob>();
            builder.Services.AddSingleton<SyncScheduler>();


            builder.Services.AddHostedService<Worker>();
            

            var host = builder.Build();
            host.Run();
        }
    }
}