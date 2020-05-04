using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using Xunit;
using Xunit.Abstractions;
using static Arbor.Docker.PortMapping;

namespace Arbor.Docker.Xunit.Tests.Integration
{
    public class SampleDockerTest : DockerTest
    {
        public SampleDockerTest(ITestOutputHelper outputHelper)
            : base(outputHelper.ToLogger())
        {
        }

        protected override async IAsyncEnumerable<ContainerArgs> AddContainersAsync()
        {
            var portMappings = new[] {new PortMapping(new PortRange(3125), new PortRange(80)), MapSinglePort(2526, 25)};
            var smtp4Dev = new ContainerArgs(
                "rnwood/smtp4dev:linux-amd64-v3",
                "smtp4devtest",
                portMappings,
                new Dictionary<string, string> {["ServerOptions:TlsMode"] = "None"}
            );

            yield return smtp4Dev;
        }

        private async Task SendMail()
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("test@test.local"));
            message.To.Add(new MailboxAddress("test@test.local"));
            message.Subject = "testsubject";

            message.Body = new TextPart("plain") {Text = "test"};

            Exception? exception = default;
            try
            {
                using var client = new SmtpClient();

                await client.ConnectAsync("localhost", 2526, false);

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            if (exception is { })
            {
                Context.Logger.Error(exception, "Failed to send email");
            }

            Assert.Null(exception);
        }

        [Fact]
        public async Task Do() => await SendMail();
    }
}