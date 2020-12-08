using System;
using System.Reflection;
using GenModelMetadataType.Services;
using Microsoft.Extensions.DependencyInjection;

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

            var serviceProvider = new ServiceCollection()
                .AddTransient<App>(x => new App(
                    x.GetRequiredService<IPathService>(),
                    x.GetRequiredService<IFileService>(),
                    dbContextName))
                .AddTransient<IPathService, PathService>()
                .AddTransient<IFileService, FileService>()
                .BuildServiceProvider();

            serviceProvider.GetRequiredService<App>().Run();
        }
    }
}