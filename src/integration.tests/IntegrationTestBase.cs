using System.Text.Json;

using Crash.Changes;
using Crash.Common.Changes;
using Crash.Common.Communications;
using Crash.Common.Document;
using Crash.Geometry;
using Crash.Handlers.Changes;
using Crash.Server;
using Crash.Server.Model;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using User = Crash.Common.Document.User;

namespace integration.tests
{
	public abstract class IntegrationTestBase
	{
		private const string url = "http://0.0.0.0:8080";
		private const string uurl = "http://localhost:8080/Crash";

		protected WebApplication App;

		protected int Clients = 0;
		protected Dictionary<int, bool> Connected = new();

		protected CrashContext CurrentContext;


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

				doc.Users.CurrentUser = new User(GetUser(i));

				LocalDocuments[i] = doc;

				Connected[i] = false;
			}
		}

		private string GetUser(int index)
		{
			if (index < Users.Count)
			{
				return Users[index];
			}

			return Path.GetRandomFileName().Replace(".", "");
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

			CurrentContext = null;
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
				doc.LocalClient = new CrashClient(doc, GetUser(index), new Uri(uurl));

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
			CurrentContext = App.Services.CreateScope().ServiceProvider.GetService<CrashContext>();
		}

		#region Crash Server

		#endregion

		#region Utils

		protected async Task PushNewChangeToServer(int index, Guid changeId, string user)
		{
			await LocalDocuments[index].LocalClient.PushChangeAsync(
				GeometryChange.CreateChange(changeId, user,
					ChangeAction.Add | ChangeAction.Temporary,
					"Test!"
				));
		}

		protected async Task TransformExistingChangeOnServer(int index, Guid changeId, string user)
		{
			var transformPayload = JsonSerializer.Serialize(new CTransform(Enumerable.Repeat(100.0, 16).ToArray()));
			await LocalDocuments[index].LocalClient.PushChangeAsync(
				GeometryChange.CreateChange(changeId, user,
					ChangeAction.Transform,
					transformPayload
				));
		}

		protected async Task AppendUpdateToExistingChangeOnServer(int index, Guid changeId, string user)
		{
			var updateData = new Dictionary<string, string> { { "Key", "Value" } };
			var updatePayload = JsonSerializer.Serialize(updateData);
			await LocalDocuments[index].LocalClient.PushChangeAsync(
				GeometryChange.CreateChange(changeId, user,
					ChangeAction.Update,
					updatePayload
				));
		}

		protected async Task ReleaseChangesByUser(int index, string user)
		{
			await LocalDocuments[index].LocalClient.PushChangeAsync(
				DoneChange.GetDoneChange(user));
		}

		#endregion
	}
}
