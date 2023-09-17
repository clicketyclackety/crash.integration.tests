using Crash.Common.Communications;
using Crash.Common.Document;
using Crash.Server;

using Microsoft.AspNetCore.Builder;

namespace integration.tests
{
	public abstract class IntegrationTestBase
	{
		private const string url = "http://0.0.0.0:8080";
		private const string uurl = "http://localhost:8080/Crash";

		protected WebApplication App;

		protected int Clients = 0;
		protected Dictionary<int, bool> Connected = new();

		private bool Initialized;

		protected Dictionary<int, CrashDoc> LocalDocuments = new();

		protected Dictionary<int, string> Users = new()
		{
			{ 0, "Curtis" }, { 1, "Lukas" }, { 2, "Callum" }, { 3, "Morteza" }
		};

		private bool UsersInitialized;

		protected void SetupCrashDocs(int count)
		{
			for (var i = 0; i < count; i++)
			{
				var doc = new CrashDoc();
				doc.Users.CurrentUser = new User(Users[i]);

				LocalDocuments[i] = doc;

				Connected[i] = false;
			}
		}

		[SetUp]
		public async Task Setup()
		{
			SetupCrashDocs(Clients);
			SetupClients();
			SetupServer();
		}

		[TearDown]
		public async Task TearDown()
		{
			foreach (var kvp in LocalDocuments)
			{
				await kvp.Value.LocalClient.StopAsync();
			}

			await App.StopAsync();

			Assert.That(Initialized, Is.True, "Server never Initialized");
			Assert.That(UsersInitialized, Is.True, "Server never Initialized Users");

			Initialized = false;
			UsersInitialized = false;
		}

		protected void SetupClients()
		{
			foreach (var (index, doc) in LocalDocuments)
			{
				doc.LocalClient = new CrashClient(doc, Users[index], new Uri(uurl));

				doc.LocalClient.OnInitializeChanges += changes =>
				{
					// Assert.That(changes, Is.Empty);
					Initialized = true;
				};

				doc.LocalClient.OnInitializeUsers += users =>
				{
					// Assert.That(users, Is.Empty);
					UsersInitialized = true;
				};
			}
		}

		protected void SetupServer()
		{
			App = Program.CreateApplication($"--urls {url}");
			App.RunAsync();
		}
	}
}
