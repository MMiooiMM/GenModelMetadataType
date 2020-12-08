using System;
using System.Reflection;
using GenModelMetadataType.Services;

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

            IPathService pathService = new PathService();
            IFileService fileService = new FileService();

            var (path, name) = pathService.GetAssemblyPathInfo();

            var assembly = fileService.GetAssembly(path, name);

            var types = fileService.GetEntityTypesFromAssembly(assembly, dbContextName);

            fileService.CreateFiles(types);

            Console.WriteLine("Done.");
        }
    }
}