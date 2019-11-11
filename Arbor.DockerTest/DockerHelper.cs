using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Arbor.Processing;

using Serilog;

namespace Arbor.DockerTest
{
    public static class DockerHelper
    {
        public static async Task<ExitCode> RunDockerCommandsAsync(
            IEnumerable<string> args,
            ILogger logger,
            CancellationToken token = default)
        {
            string process = @"C:\Program Files\Docker\Docker\Resources\bin\docker.exe";

            var exitCode = await ProcessRunner.ExecuteProcessAsync(
                               process,
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