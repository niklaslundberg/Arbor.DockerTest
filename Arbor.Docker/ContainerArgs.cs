using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using JetBrains.Annotations;

namespace Arbor.Docker
{
    public class ContainerArgs
    {
        public ContainerArgs(
            [NotNull] string imageName,
            [NotNull] string containerName,
            IDictionary<int, int> ports = null,
            IDictionary<string, string> environmentVariables = null,
            string[] args = null)
        {
            if (string.IsNullOrWhiteSpace(imageName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(imageName));
            }

            if (string.IsNullOrWhiteSpace(containerName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(containerName));
            }

            ImageName = imageName;
            ContainerName = containerName;
            Ports = ports?.ToImmutableDictionary() ?? ImmutableDictionary<int, int>.Empty;
            EnvironmentVariables = environmentVariables?.ToImmutableDictionary() ??
                                   ImmutableDictionary<string, string>.Empty;
            Args = args?.ToImmutableArray() ?? ImmutableArray<string>.Empty;
        }

        public ImmutableArray<string> Args { get; }

        public string ContainerName { get; }

        public ImmutableDictionary<string, string> EnvironmentVariables { get; }

        public string ImageName { get; }

        public ImmutableDictionary<int, int> Ports { get; }

        public ImmutableArray<string> CombinedArgs()
        {
            var args = new List<string> {"run", "-d"};

            foreach (var keyValuePair in Ports)
            {
                args.Add("-p");
                args.Add($"{keyValuePair.Key}:{keyValuePair.Value}");
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

            args.Add("--platform=linux");

            args.Add(ImageName);

            return args.ToImmutableArray();
        }
    }
}