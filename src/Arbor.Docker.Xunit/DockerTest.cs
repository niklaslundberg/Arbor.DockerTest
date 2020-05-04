using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog;
using Xunit;

namespace Arbor.Docker.Xunit
{
    public abstract class DockerTest : IAsyncLifetime
    {
        private readonly ILogger _logger;

        protected DockerTest(ILogger logger) => _logger = logger;

        public DockerContext Context { get; private set; }

        public virtual async Task DisposeAsync()
        {
            await Context.SafeDisposeAsync();
            await _logger.SafeDisposeAsync();
        }

        public virtual async Task InitializeAsync()
        {
            var containers = new List<ContainerArgs>();

            await foreach (var container in AddContainersAsync())
            {
                containers.Add(container);
            }

            Context = await DockerContext.CreateContextAsync(containers, _logger);

            await Context.ContainerTask;
        }

        protected virtual async IAsyncEnumerable<ContainerArgs> AddContainersAsync()
        {
            yield break;
        }
    }
}