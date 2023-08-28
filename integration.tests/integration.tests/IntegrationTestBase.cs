﻿using Crash.Client;
using Crash.Common.Document;
using Crash.Server;

using Microsoft.AspNetCore.Builder;

using User = Crash.Common.Document.User;

namespace integration.tests
{
	public abstract class IntegrationTestBase
	{

		const string url = "http://0.0.0.0:8080";
		const string uurl = "http://localhost:8080/Crash";

		protected Dictionary<int, CrashDoc> LocalDocuments = new();
		protected Dictionary<int, string> Users = new() { { 0, "Curtis" }, { 1, "Lukas" }, { 2, "Callum" }, { 3, "Morteza" } };
		protected Dictionary<int, bool> Connected = new();

		protected WebApplication App;

		protected void SetupCrashDocs(int count)
		{
			for(int i = 0; i < count; i++)
			{
				var doc = new CrashDoc();
				doc.Users.CurrentUser = new User(Users[i]);

				LocalDocuments[i] = doc;

				Connected[i] = false;
			}
			
		}

		protected void SetupClients()
		{
			foreach(var kvp in LocalDocuments)
			{
				var doc = kvp.Value;
				int index = kvp.Key;
				doc.LocalClient = new CrashClient(doc, Users[index], new Uri(uurl));

				doc.LocalClient.OnInitialize += (changes) =>
				{
					Assert.That(changes, Is.Empty);
				};

				doc.LocalClient.OnInitializeUsers += (changes) =>
				{
					Assert.That(changes, Is.Empty);
				};
			}
		}

		protected async Task SetupServer()
		{
			App = Program.CreateApplication(new string[] { $"--urls {url}" });
			App.RunAsync();
		}

	}

}