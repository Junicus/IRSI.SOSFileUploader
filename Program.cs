using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using IRSI.SOSFileUploader.Configuration;
using Microsoft.Extensions.Configuration;

namespace IRSI.SOSFileUploader
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var assembly = Assembly.GetEntryAssembly();
            var builder = new ContainerBuilder();

            var cbuiler = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables();
            var config = cbuiler.Build();

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
                var sosFileUploader = container.Resolve<SOSFileUploader>();
                Task.WaitAll(sosFileUploader.RunAsync());
            } catch(Exception ex)
            {
                //log ex
                Console.WriteLine(ex.Message);
            }
        }
    }
}
