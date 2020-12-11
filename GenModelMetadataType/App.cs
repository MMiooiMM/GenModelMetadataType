using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GenModelMetadataType.Properties;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GenModelMetadataType
{
    public class App : IHostedService
    {
        private static readonly string dbContextFullName = "Microsoft.EntityFrameworkCore.DbContext";
        private int? _exitCode;

        private readonly ILogger<App> logger;
        private readonly IHostApplicationLifetime appLifetime;

        public App(ILogger<App> logger, IHostApplicationLifetime appLifetime)
        {
            this.logger = logger;
            this.appLifetime = appLifetime;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                logger.LogWarning("Worker running at: {time}", DateTimeOffset.Now);

                var runner = new CommandRunner("dotnet genmodelmetadatatype", "GenModelMetadataType Command Line Tools", Console.Out);

                runner.SubCommand("list", "show dbContext type list ", c =>
                {
                    c.Option("project", "project", "p", "project description");
                    c.OnRun((namedArgs) =>
                    {
                        var project = GetAndBuildProject(namedArgs.GetValueOrDefault("project"));

                        var assembly = GetAssemblyFromProject(project);

                        var dbContextNames = GetDbContextTypesFromAssembly(assembly).ToList().Select(type => GetFullName(type));

                        var message = string.Join("\n", dbContextNames);

                        logger.LogInformation(message);

                        return 1;
                    });
                });

                runner.SubCommand("generate", "generate partial code ", c =>
                {
                    c.Option("output", "output", "o", "output description");
                    c.Option("project", "project", "p", "project description");
                    c.Option("context", "context", "c", "context description");
                    c.OnRun((namedArgs) =>
                    {
                        var project = GetAndBuildProject(namedArgs.GetValueOrDefault("project"));                       

                        var assembly = GetAssemblyFromProject(project);

                        var types = GetEntityTypesFromAssembly(assembly, namedArgs.GetValueOrDefault("output"));

                        CreateFiles(types, namedArgs.GetValueOrDefault("context"));

                        return 1;
                    });
                });

                _exitCode = runner.Run(Environment.GetCommandLineArgs().Skip(1));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception!");
                _exitCode = 1;
            }
            finally
            {
                appLifetime.StopApplication();
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogWarning("Worker stopped at: {time}", DateTimeOffset.Now);

            Environment.ExitCode = _exitCode.GetValueOrDefault(-1);

            return Task.CompletedTask;
        }

        private Project GetAndBuildProject(string projectPath)
        {
            var projectFile = ResolveProject(projectPath);

            var project = Project.FromFile(projectFile, null);

            logger.LogInformation(Resources.BuildStarted);
            project.Build();
            logger.LogInformation(Resources.BuildSucceeded);

            return project;
        }

        private string ResolveProject(string projectPath)
        {
            var projects = GetProjectFiles(projectPath);

            return projects.Count switch
            {
                0 => throw new Exception(projectPath != null
                    ? Resources.NoProjectInDirectory(projectPath)
                    : Resources.NoProject),
                > 1 => throw new Exception(projectPath != null
                    ? Resources.MultipleProjectsInDirectory(projectPath)
                    : Resources.MultipleProjects),
                _ => projects[0],
            };
        }

        private List<string> GetProjectFiles(string path)
        {
            if (path == null)
            {
                path = Directory.GetCurrentDirectory();
            }
            else if (!Path.IsPathRooted(path))
            {
                path = Path.Combine(Directory.GetCurrentDirectory(), path);
            }

            if (!Directory.Exists(path))
            {
                return new List<string> { path };
            }

            var projectFiles = Directory.EnumerateFiles(path, "*.*proj", SearchOption.TopDirectoryOnly)
                .Where(f => !string.Equals(Path.GetExtension(f), ".xproj", StringComparison.OrdinalIgnoreCase))
                .Take(2).ToList();

            return projectFiles;
        }

        /// <summary>
        /// 透過 project 取得 Assembly
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private Assembly GetAssemblyFromProject(Project project)
        {
            var targetDir = Path.GetFullPath(Path.Combine(project.ProjectDir, project.OutputPath));

            string localPath = string.IsNullOrEmpty(targetDir)
                ? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                : targetDir;

            string assemblyFilePath = Path.Combine(localPath, project.TargetFileName);

            if (!File.Exists(assemblyFilePath))
            {
                throw new Exception($"Missing {assemblyFilePath}");
            }

            return Assembly.LoadFrom(assemblyFilePath);
        }

        /// <summary>
        /// 從 Assembly 取得所有 Entity 的 Type
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private IEnumerable<Type> GetEntityTypesFromAssembly(Assembly assembly, string context)
        {
            return GetDbContextTypesFromAssembly(assembly)
                .FirstOrDefault(t => t.FullName.Equals(context))
                .GetProperties()
                .Where(prop => CheckIfDbSetGenericType(prop.PropertyType))
                .Select(type => type.PropertyType.GetGenericArguments()[0]);
        }

        /// <summary>
        /// 新增檔案
        /// </summary>
        /// <param name="types"></param>
        /// <param name="output"></param>
        private void CreateFiles(IEnumerable<Type> types, string output)
        {
            var outputDir = Path.Combine(Directory.GetCurrentDirectory(), output);

            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            foreach (var type in types)
            {
                var fileName = $"{type.Name}.Partial.cs";
                var fileContent = GeneratePartialCodeContent(type);

                using StreamWriter sw = new StreamWriter(Path.Combine(outputDir, fileName));
                sw.Write(fileContent);

                logger.LogInformation($"create {fileName}.");
            }
        }

        /// <summary>
        /// 從 Assembly 取得 DbContext 的 Type
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private IEnumerable<Type> GetDbContextTypesFromAssembly(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null && t.BaseType.FullName.Contains(dbContextFullName));
            }
        }

        /// <summary>
        /// 檢查是否為 DbSet 的泛型類別
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool CheckIfDbSetGenericType(Type type)
        {
            return type.IsGenericType && GetFullName(type).Contains("DbSet");
        }

        /// <summary>
        /// 取得 Type 的完整名稱
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string GetFullName(Type type)
        {
            if (!type.IsGenericType) return type.Name;

            StringBuilder sb = new StringBuilder();

            sb.Append(type.Name.Substring(0, type.Name.LastIndexOf("`")));
            sb.Append(type.GetGenericArguments().Aggregate("<",
                delegate (string aggregate, Type type)
                {
                    return aggregate + (aggregate == "<" ? "" : ",") + GetFullName(type);
                }));
            sb.Append(">");

            return sb.ToString();
        }

        /// <summary>
        /// 產生 Partial Code 內容
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string GeneratePartialCodeContent(Type type)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("using Microsoft.AspNetCore.Mvc;");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.ComponentModel.DataAnnotations;");
            sb.AppendLine();
            sb.AppendLine("#nullable disable");
            sb.AppendLine();
            sb.AppendLine($"namespace {type.Namespace}");
            sb.AppendLine("{");
            sb.AppendLine($"    [ModelMetadataType(typeof({type.Name}Metadata))]");
            sb.AppendLine($"    public partial class {type.Name}");
            sb.AppendLine("    {");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine($"    internal class {type.Name}Metadata");
            sb.AppendLine("    {");

            foreach (var prop in type.GetProperties().Where(t => !t.GetGetMethod().IsVirtual))
            {
                sb.AppendLine("        // [Required]");
                sb.AppendLine($"        public {GetFullName(prop.PropertyType)} {prop.Name} {{ get; set; }}");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }
    }
}