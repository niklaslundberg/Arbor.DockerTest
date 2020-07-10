using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            bool logAsDebug = false,
            CancellationToken token = default)
        {
            var candidatePaths = new List<string>(2)
            {
                @"C:\Program Files\Docker\docker.exe",
                @"C:\Program Files\Docker\Docker\Resources\bin\docker.exe"
            };

            dockerExePath ??= candidatePaths.FirstOrDefault(File.Exists);

            var exePath = new FileInfo(dockerExePath);

            if (!exePath.Exists)
            {
                throw new InvalidOperationException($"The docker exe file '{exePath.FullName}' does not exist");
            }

            void LogDebug(string message, string category)
            {
                logger.Debug("{Message}", message);
            }

            void LogError(string message, string _)
            {
                logger.Error("{Message}", message);
            }

            void LogInformation(string message, string _)
            {
                logger.Information("{Message}", message);
            }

            var exitCode = await ProcessRunner.ExecuteProcessAsync(
                exePath.FullName,
                args,
                logAsDebug
                    ? LogDebug
                    : (CategoryLog)LogInformation,
                logAsDebug
                    ? (CategoryLog)LogDebug
                    : LogError,
                debugAction: LogDebug,
                verboseAction: (message, category) => logger.Verbose("{Message}", message),
                toolAction: LogDebug,
                formatArgs: false,
                cancellationToken: token);

            if (!exitCode.IsSuccess)
            {
                logger.Error("Docker failed");
            }

            return exitCode;
        }
    }
}