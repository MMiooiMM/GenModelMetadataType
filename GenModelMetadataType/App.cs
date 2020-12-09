using System;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly IPathService pathService;
        private readonly IFileService fileService;

        public App(ILogger<App> logger,
                   IHostApplicationLifetime appLifetime,
                   IConfiguration configuration,
                   IPathService pathService,
                   IFileService fileService)
        {
            this.logger = logger;
            this.appLifetime = appLifetime;
            this.pathService = pathService;
            this.fileService = fileService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                logger.LogWarning("Worker running at: {time}", DateTimeOffset.Now);

                var (path, name) = pathService.GetAssemblyFileInfo();

                fileService.CreatePartialFiles(path, name);

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
    }
}