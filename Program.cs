using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using IRSI.SOSFileUploader.Configuration;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace IRSI.SOSFileUploader
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var assembly = Assembly.GetEntryAssembly();
            var builder = new ContainerBuilder();

            var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

            var cbuiler = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables();
            var config = cbuiler.Build();

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.ColoredConsole()
                .WriteTo.RollingFile(config["SOSFileUploader:LogPath"] + "/Log-{Date}.txt")
                .CreateLogger();

            ILogger log = Log.ForContext<Program>();
            log.Information("Registering Components and Modules...");

            builder.Register<SOSApiClientOptions>(c => new SOSApiClientOptions()
            {
                ApiUrl = config["SOSApi:ApiUrl"],
                ClientId = config["SOSApi:ClientId"],
                ClientSecret = config["SOSApi:ClientSecret"]
            });
            builder.Register<TokenClientOptions>(t => new TokenClientOptions()
            {
                TokenUrl = config["IdentityServer:TokenEndpointUrl"]
            });
            builder.Register<SOSFileUploaderOptions>(s => new SOSFileUploaderOptions
            {
                StoreId = Guid.Parse(config["SOSFileUploader:StoreId"]),
                QsrSOSPath = config["SOSFileUploader:QsrSOSPath"]
            });
            builder.Register<FileHistoryServiceOptions>(h => new FileHistoryServiceOptions
            {
                HistoryFilePath = config["SOSFileUploader:HistoryFilePath"]
            });
            builder.RegisterAssemblyModules(assembly);
            var container = builder.Build();
            try
            {
                log.Information("Resolving SOSFileUploader...");
                var sosFileUploader = container.Resolve<SOSFileUploader>();
                log.Information("SOSFileUploader resolved.");
                log.Information("Running SOSFileUploader...");
                Task.WaitAll(sosFileUploader.RunAsync());
                log.Information("SOSFileUploader finished running successfully");
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error resolving or running SOSFileUploader");
                Console.WriteLine(ex.Message);
            }
        }
    }
}
