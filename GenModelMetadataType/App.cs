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

        public App(ILogger<App> logger,
                   IHostApplicationLifetime appLifetime,
                   IFileService fileService)
        {
            this.logger = logger;
            this.appLifetime = appLifetime;
            this.fileService = fileService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                logger.LogWarning("Worker running at: {time}", DateTimeOffset.Now);

                var (projectFile, startupProjectFile) = ResolveProjects(null, null);

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

        private (string, string) ResolveProjects(
            string projectPath,
            string startupProjectPath)
        {
            var projects = ResolveProjects(projectPath);
            var startupProjects = ResolveProjects(startupProjectPath);

            if (projects.Count > 1)
            {
                throw new Exception(
                    projectPath != null
                        ? Resources.MultipleProjectsInDirectory(projectPath)
                        : Resources.MultipleProjects);
            }

            if (startupProjects.Count > 1)
            {
                throw new Exception(
                    startupProjectPath != null
                        ? Resources.MultipleProjectsInDirectory(startupProjectPath)
                        : Resources.MultipleStartupProjects);
            }

            if (projectPath != null
                && projects.Count == 0)
            {
                throw new Exception(Resources.NoProjectInDirectory(projectPath));
            }

            if (startupProjectPath != null
                && startupProjects.Count == 0)
            {
                throw new Exception(Resources.NoProjectInDirectory(startupProjectPath));
            }

            if (projectPath == null
                && startupProjectPath == null)
            {
                return projects.Count == 0
                    ? throw new Exception(Resources.NoProject)
                    : (projects[0], startupProjects[0]);
            }

            if (projects.Count == 0)
            {
                return (startupProjects[0], startupProjects[0]);
            }

            if (startupProjects.Count == 0)
            {
                return (projects[0], projects[0]);
            }

            return (projects[0], startupProjects[0]);
        }

        private List<string> ResolveProjects(string path)
        {
            if (path == null)
            {
                path = Directory.GetCurrentDirectory();
            }
            else if (!Directory.Exists(path))
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