using Griffin.DataSync.Service.Configuration;
using Griffin.DataSync.Service.Helpers;
using Griffin.DataSync.Service.Infrastructure.ConnectionFactories;
using Griffin.DataSync.Service.Interfaces;
using Griffin.DataSync.Service.Jobs;
using Griffin.DataSync.Service.Models;
using Griffin.DataSync.Service.Repositories;
using Griffin.DataSync.Service.Services;
using Serilog;

namespace Griffin.DataSync.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddWindowsService(opt =>
            {
                opt.ServiceName = "Griffin Data Sync Service";
            });
            builder.Services.AddHttpClient(
            "MicrosoftGraph",
            client =>
            {
                client.BaseAddress = new Uri(
                    "https://graph.microsoft.com/v1.0/");
            
                client.Timeout = TimeSpan.FromSeconds(
                    builder.Configuration
                        .GetSection("Email")
                        .GetValue<int>("TimeoutSeconds", 30));
            });
            builder.Services.Configure<SyncOptions>(
            builder.Configuration.GetSection("Sync"));
            builder.Services.Configure<JobScheduleOptions>(
            builder.Configuration);

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(
                     @"C:\Logs\GriffinDataSync\log-.txt",
                    rollingInterval: RollingInterval.Day)
                .CreateLogger();
            builder.Services.AddSerilog();
            var tableDefinitions =
                builder.Configuration
                    .GetSection("Sync:Tables")
                    .Get<List<TableSyncDefinition>>()
                    ?? new List<TableSyncDefinition>();
            
                      foreach (var definition in tableDefinitions)
            {
                builder.Services.AddSingleton<ISyncJob>(sp =>
                    ActivatorUtilities.CreateInstance<TableSyncJob>(
                        sp,
                        definition));
            }
            builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));
            //builder.Services.AddSingleton<IEmailService, EmailService>();
            //builder.Services.AddSingleton<ISyncJob, PickExceptionDetectionJob>();
            builder.Services.AddSingleton<ISyncJob, SendEmailAlertsJob>();
            builder.Services.AddSingleton<ISyncJob, UpdateShipperBoardJob>();
            builder.Services.AddSingleton<ISqlQueryProvider, SqlQueryProvider>();
            builder.Services.AddSingleton<IOdbcConnectionFactory,OdbcConnectionFactory>();
            //builder.Services.AddSingleton<ISyncJob,InventoryReplenishmentSyncJob>();
            builder.Services.AddSingleton<ISqlConnectionFactory,SqlConnectionFactory>();
            builder.Services.AddSingleton<ISageRepository, SageRepository>();
            //builder.Services.AddSingleton<ISyncJob, CIItemSyncJob>();
            builder.Services.AddSingleton<SageConnectorRunner>();
            builder.Services.AddSingleton<ISqlRepo, SqlRepo>();
            builder.Services.AddSingleton<ISyncEngine, SyncEngine>();
            builder.Services.AddSingleton<RetryService>();
            //builder.Services.AddSingleton<ISyncJob, MBBinLocationSyncJob>();
            builder.Services.AddSingleton<SyncScheduler>();


            builder.Services.AddHostedService<Worker>();          

            var host = builder.Build();
            host.Run();
        }
    }
}