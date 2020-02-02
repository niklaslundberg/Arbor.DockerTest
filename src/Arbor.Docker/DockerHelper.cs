using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Arbor.Processing;
using Serilog;

namespace Arbor.Docker
{
    public static class DockerHelper
    {
        public static async Task<ExitCode> RunDockerCommandsAsync(
            IEnumerable<string> args,
            ILogger logger,
            string? dockerExePath = null,
            CancellationToken token = default)
        {
            dockerExePath ??= @"C:\Program Files\Docker\Docker\Resources\bin\docker.exe";

            if (!File.Exists(dockerExePath))
            {
                throw new InvalidOperationException($"The docker exe file '{dockerExePath}' does not exist");
            }

            var exitCode = await ProcessRunner.ExecuteProcessAsync(
                dockerExePath,
                args,
                (message, category) => logger.Information("{Message}", message),
                (message, category) => logger.Error("{Message}", message),
                debugAction: (message, category) => logger.Debug("{Message}", message),
                verboseAction: (message, category) => logger.Verbose("{Message}", message),
                toolAction: (message, category) => logger.Debug("{Message}", message),
                cancellationToken: token);

            if (!exitCode.IsSuccess)
            {
                logger.Error("Docker failed");
            }

            return exitCode;
        }
    }
}