using Crash.Client;
using Crash.Common.Document;
using Crash.Server;
using Crash.Server.Hubs;
using Crash.Server.Model;
using Microsoft.EntityFrameworkCore;
using User = Crash.Common.Document.User;

namespace integration.tests
{

    public class Tests
    {

        [SetUp]
        public void Setup()
        {

            CrashDoc crashDoc = new CrashDoc();

            ;
        }

        [Test]
        public async Task Test1()
        {
            var url = "http://0.0.0.0:8080";
            var uurl = "http://localhost:8080/Crash";

            var app = Program.CreateApplication(new string[] { $"--urls {url}" });
            var application = app.Application;
            var context = app.Context;

            var crashDoc = new CrashDoc();
            crashDoc.Users.CurrentUser = new User("Me");
            crashDoc.LocalClient = new CrashClient(crashDoc, "me", new Uri(uurl));

            crashDoc.LocalClient.OnInitialize += (changes) =>
            {
                Assert.That(changes, Is.Empty);
            };

            crashDoc.LocalClient.OnInitializeUsers += (changes) =>
            {
                Assert.That(changes, Is.Empty);
            };

            await crashDoc.LocalClient.StartLocalClientAsync();

            ;
        }

    }

}