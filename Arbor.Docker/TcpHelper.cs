using System;
using System.Collections.Immutable;
using System.Linq;
using System.Net.NetworkInformation;

namespace Arbor.Docker
{
    public static class TcpHelper
    {
        public static int GetAvailablePort(int startingPort)
        {
            ushort maxValue = ushort.MaxValue;
            if (startingPort < 0 || startingPort > maxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(startingPort));
            }

            var properties = IPGlobalProperties.GetIPGlobalProperties();

            int[] tcpConnectionPorts = properties.GetActiveTcpConnections()
                .Where(n => n.LocalEndPoint.Port >= startingPort)
                .Select(n => n.LocalEndPoint.Port)
                .ToArray();

            int[] tcpListenerPorts = properties.GetActiveTcpListeners()
                .Where(ipEndPoint => ipEndPoint.Port >= startingPort)
                .Select(ipEndPoint => ipEndPoint.Port)
                .ToArray();

            int[] udpListenerPorts = properties.GetActiveUdpListeners()
                .Where(ipEndPoint => ipEndPoint.Port >= startingPort)
                .Select(ipEndPoint => ipEndPoint.Port)
                .ToArray();

            var allUsedPorts = tcpConnectionPorts
                .Concat(tcpListenerPorts)
                .Concat(udpListenerPorts)
                .ToImmutableHashSet();

            int port = Enumerable
                .Range(startingPort, maxValue)
                .First(currentPort => !allUsedPorts.Contains(currentPort));

            return port;
        }
    }
}