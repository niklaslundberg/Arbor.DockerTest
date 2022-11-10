using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using Xunit;
using Xunit.Abstractions;

namespace Arbor.Docker.Xunit.Tests.Integration
{
    public class RedisTest : DockerTest
    {
        public RedisTest(ITestOutputHelper outputHelper)
            : base(outputHelper.ToLogger())
        {
        }

        protected override async IAsyncEnumerable<ContainerArgs> AddContainersAsync()
        {
            var portMappings = new[] {PortMapping.MapSinglePort(36379, 6379)};
            var redis = new ContainerArgs(
                "redis",
                "redistest",
                portMappings,
                entryPoint: new[] {"redis-server", "--appendonly yes"}
            );

            yield return redis;
        }

        [Fact]
        public async Task SetAndGetFromDistributedCache()
        {
            var redisCacheOptions = new RedisCacheOptions {Configuration = "localhost:36379"};
            IDistributedCache distributedCache =
                new RedisCache(new OptionsWrapper<RedisCacheOptions>(redisCacheOptions));

            await distributedCache.SetStringAsync("test", "hello world");

            string cachedValue = await distributedCache.GetStringAsync("test");

            Assert.Equal("hello world", cachedValue);
        }
    }
}