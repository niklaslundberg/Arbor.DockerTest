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
            yield return new ContainerArgs(
                "rnwood/smtp4dev:linux-amd64-v3",
                "smtp4devtest",
                portMappings,
                new Dictionary<string, string> {["ServerOptions:TlsMode"] = "None"}
            );
        }

        [Fact(Skip = "Environment dependent")]
        public async Task SendMail()
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse("test@test.local"));
            message.To.Add(MailboxAddress.Parse("test@test.local"));
            message.Subject = "testsubject";

            message.Body = new TextPart("plain") {Text = "test"};

            Exception? exception = default;
            try
            {
                using var client = new SmtpClient();

                client.AuthenticationMechanisms.Remove("XOAUTH2");

                await client.ConnectAsync("localhost", 2526, false).ConfigureAwait(false);

                await client.SendAsync(message).ConfigureAwait(false);
                await client.DisconnectAsync(true).ConfigureAwait(false);
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
    }
}