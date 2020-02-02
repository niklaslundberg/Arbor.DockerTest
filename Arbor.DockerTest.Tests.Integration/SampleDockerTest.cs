using System.Collections.Generic;
using System.Threading.Tasks;
using Arbor.Docker;
using MailKit.Net.Smtp;

using MimeKit;

using Xunit;
using Xunit.Abstractions;

namespace Arbor.DockerTest.Tests.Integration
{
    public class SampleDockerTest : DockerTest
    {
        public SampleDockerTest(ITestOutputHelper outputHelper)
            : base(outputHelper.ToLogger())
        {
        }

        [Fact]
        public async Task Do() => await SendMail();

        protected override async IAsyncEnumerable<ContainerArgs> AddContainersAsync()
        {
            var smtp4Dev = new ContainerArgs(
                "rnwood/smtp4dev:linux-amd64-v3",
                "smtp4devtest",
                new Dictionary<int, int> { [3125] = 80, [2526] = 25 }
            );

            yield return smtp4Dev;
        }

        private async Task SendMail()
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("test@test.local"));
            message.To.Add(new MailboxAddress("test@test.local"));
            message.Subject = "testsubject";

            message.Body = new TextPart("plain") { Text = "test" };

            using var client = new SmtpClient();
            await client.ConnectAsync("localhost", 2526, false);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}