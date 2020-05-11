using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;

namespace Arbor.Docker
{
    public class ContainerArgs
    {
        private readonly bool _useExplicitPlatform;

        public ContainerArgs(
            [NotNull] string imageName,
            [NotNull] string containerName,
            IEnumerable<PortMapping>? ports = null,
            IDictionary<string, string>? environmentVariables = null,
            string[]? args = null,
            string[]? entryPoint = null,
            bool useExplicitPlatform = false)
        {
            if (string.IsNullOrWhiteSpace(imageName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(imageName));
            }

            if (string.IsNullOrWhiteSpace(containerName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(containerName));
            }

            _useExplicitPlatform = useExplicitPlatform;

            ImageName = imageName;
            ContainerName = containerName;
            Ports = ports?.ToImmutableArray() ?? ImmutableArray<PortMapping>.Empty;

            EnvironmentVariables = environmentVariables?.ToImmutableDictionary() ??
                                   ImmutableDictionary<string, string>.Empty;

            Args = args?.ToImmutableArray() ?? ImmutableArray<string>.Empty;
            EntryPoint = entryPoint?.ToImmutableArray() ?? ImmutableArray<string>.Empty;
        }

        public ImmutableArray<string> EntryPoint { get; set; }

        public ImmutableArray<string> Args { get; }

        public string ContainerName { get; }

        public ImmutableDictionary<string, string> EnvironmentVariables { get; }

        public string ImageName { get; }

        public ImmutableArray<PortMapping> Ports { get; }

        public ImmutableArray<string> StartArguments()
        {
            return CombinedArgs().Concat(EntryPoint).ToImmutableArray();
        }

        public ImmutableArray<string> CombinedArgs()
        {
            var args = new List<string> {"run", "-d"};

            foreach (var range in Ports)
            {
                args.Add("-p");

                if (range.HostPorts.End > range.HostPorts.Start)
                {
                    args.Add(
                        $"{range.HostPorts.Start}-{range.HostPorts.End}:{range.ContainerPorts.Start}-{range.ContainerPorts.End}");
                }
                else
                {
                    args.Add($"{range.HostPorts.Start}:{range.ContainerPorts.Start}");
                }
            }

            foreach (var keyValuePair in EnvironmentVariables)
            {
                args.Add("-e");
                args.Add($"{keyValuePair.Key}={keyValuePair.Value}");
            }

            foreach (string arg in Args)
            {
                args.Add(arg);
            }

            args.Add("--name");
            args.Add(ContainerName);

            if (_useExplicitPlatform)
            {
                args.Add("--platform=linux");
            }

            args.Add(ImageName);

            return args.ToImmutableArray();
        }
    }
}