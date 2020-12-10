using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GenModelMetadataType.Properties;
using GenModelMetadataType.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GenModelMetadataType
{
    public class App : IHostedService
    {
        private int? _exitCode;

        private readonly ILogger<App> logger;
        private readonly IHostApplicationLifetime appLifetime;
        private readonly IFileService fileService;
        private readonly IConfiguration configuration;

        public App(ILogger<App> logger,
                   IHostApplicationLifetime appLifetime,
                   IFileService fileService,
                   IConfiguration configuration)
        {
            this.logger = logger;
            this.appLifetime = appLifetime;
            this.fileService = fileService;
            this.configuration = configuration;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                logger.LogWarning("Worker running at: {time}", DateTimeOffset.Now);

                var projectFile = ResolveProject(configuration["project"]);

                var project = Project.FromFile(projectFile, null);

                logger.LogInformation(Resources.BuildStarted);
                project.Build();
                logger.LogInformation(Resources.BuildSucceeded);

                fileService.CreatePartialFiles(Path.Combine(project.ProjectDir, project.OutputPath), project.TargetFileName);

                _exitCode = 1;
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
    }
}