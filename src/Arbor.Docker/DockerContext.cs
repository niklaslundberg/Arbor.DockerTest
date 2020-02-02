using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Arbor.Docker
{
    public class DockerContext : IAsyncDisposable
    {
        private readonly IReadOnlyCollection<ContainerArgs> _containers;

        private readonly ILogger _logger;

        private readonly Task _task;

        private DockerContext(
            Task task,
            IReadOnlyCollection<ContainerArgs> containers,
            ILogger logger,
            CancellationTokenSource cancellationTokenSource)
        {
            _task = task;
            _containers = containers;
            _logger = logger;
            CancellationTokenSource = cancellationTokenSource;
        }

        public CancellationTokenSource CancellationTokenSource { get; }

        public async ValueTask DisposeAsync()
        {
            CancellationTokenSource?.Cancel(false);

            try
            {
                await _task;
            }
            catch (TaskCanceledException)
            {
            }
            finally
            {
                await Task.Delay(TimeSpan.FromSeconds(3));

                await ShutdownContainersAsync(_containers, _logger);
            }

            CancellationTokenSource?.Dispose();
        }

        public static async Task<DockerContext> CreateContextAsync(
            IReadOnlyCollection<ContainerArgs> args,
            ILogger logger)
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var dockerTask = Task.Run(
                () => StartAllDockerContainers(args, logger, cancellationTokenSource.Token),
                cancellationTokenSource.Token);

            return new DockerContext(dockerTask, args, logger, cancellationTokenSource);
        }

        private static async Task ShutDownAndRemoveAsync(string containerName, ILogger logger)
        {
            var args = new List<string> {"stop", containerName};

            var stopExitCode = await DockerHelper.RunDockerCommandsAsync(args, logger);

            logger.Information("Stop exit code: {StopExitCode}", stopExitCode);

            var removeArgs = new List<string> {"rm", containerName};

            var removeExitCode = await DockerHelper.RunDockerCommandsAsync(removeArgs, logger);

            logger.Information("Remove exit code: {RemoveExitCode}", removeExitCode);
        }

        private static async Task ShutdownContainersAsync(IReadOnlyCollection<ContainerArgs> containers, ILogger logger)
        {
            foreach (var container in containers)
            {
                await ShutDownAndRemoveAsync(container.ContainerName, logger);
            }
        }

        private static async Task StartAllDockerContainers(
            IReadOnlyCollection<ContainerArgs> containers,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            await ShutdownContainersAsync(containers, logger);

            var tasks = containers.Select(
                    containerArgs =>
                        DockerHelper.RunDockerCommandsAsync(containerArgs.CombinedArgs(), logger, cancellationToken))
                .ToImmutableArray();

            await Task.WhenAll(tasks.ToArray());
        }
    }
}