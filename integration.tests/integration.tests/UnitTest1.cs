using Crash.Client;
using Crash.Common.Document;
using Crash.Server;
using Crash.Server.Hubs;
using Crash.Server.Model;

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using User = Crash.Common.Document.User;

namespace integration.tests
{

    public class Tests : IDisposable
	{
		const string url = "http://0.0.0.0:8080";
		const string uurl = "http://localhost:8080/Crash";

		private CrashDoc Doc;
		private WebApplication App;

		public void Dispose()
		{
			
		}

		[SetUp]
        public void Setup()
        {
            Doc = new CrashDoc();
			Doc.Users.CurrentUser = new User("Me");
			Doc.LocalClient = new CrashClient(Doc, "me", new Uri(uurl));

			App = Program.CreateApplication(new string[] { $"--urls {url}" });
			App.RunAsync();
		}

		[TearDown]
		public async Task TearDown()
		{
			await Doc.LocalClient.StopAsync();
			await App.StopAsync();
		}

        [Test]
        public async Task Test1()
        {

			Doc.LocalClient.OnInitialize += (changes) =>
            {
                Assert.That(changes, Is.Empty);
            };

			Doc.LocalClient.OnInitializeUsers += (changes) =>
            {
                Assert.That(changes, Is.Empty);
            };

            await Doc.LocalClient.StartLocalClientAsync();

            ;
        }

    }

}
