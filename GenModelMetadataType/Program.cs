using System;
using System.Reflection;
using GenModelMetadataType.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GenModelMetadataType
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                var versionString = Assembly.GetEntryAssembly()
                                        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                                        .InformationalVersion
                                        .ToString();

                Console.WriteLine($"genmodelmetadata v{versionString}");
                Console.WriteLine("-------------");
                Console.WriteLine("\nUsage:");
                Console.WriteLine("  genmodelmetadata <dbcontext-name>");
                return;
            }

            string dbContextName = args[0];

            var builder = new HostBuilder().ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<Argument>(x => new Argument
                {
                    DbContextName = dbContextName
                });

                services.AddTransient<App>();
                services.AddTransient<IPathService, PathService>();
                services.AddTransient<IFileService, FileService>();

                services.AddLogging(builder =>
                {
                    builder.AddConsole();
                });
            });

            builder.Build().Services.GetRequiredService<App>().Run();
        }
    }
}