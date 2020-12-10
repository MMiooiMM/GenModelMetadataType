using System.Collections.Generic;
using GenModelMetadataType.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GenModelMetadataType
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<App>();
                    services.AddTransient<IFileService, FileService>();
                })
                .ConfigureLogging((hostContext, configLogging) =>
                {
                    configLogging.AddConsole();
                    configLogging.AddDebug();
                })
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                var switchMappings = new Dictionary<string, string>()
                    {
                        { "-c", "context" },
                        { "--context", "context" },
                        { "-p", "project" },
                        { "--project", "project" },
                        { "-o", "outputDir" },
                        { "--output-dir", "outputDir" }
                    };
                config.AddCommandLine(args, switchMappings);
            });
    }
}