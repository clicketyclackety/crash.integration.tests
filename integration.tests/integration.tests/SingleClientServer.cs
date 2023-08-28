using Crash.Changes;
using Crash.Server.Hubs;
using Crash.Server.Model;

using Microsoft.EntityFrameworkCore;

using Rhino.UI;

namespace integration.tests
{

	public class Tests : IntegrationTestBase
	{

		[SetUp]
        public async Task Setup()
        {
			SetupCrashDocs(1);
			SetupClients();
			SetupServer();
		}

		[TearDown]
		public async Task TearDown()
		{
			foreach(var kvp in LocalDocuments)
			{
				await kvp.Value.LocalClient.StopAsync();
			}

			await App.StopAsync();
		}

		[Test]
		public async Task Test1()
		{
			foreach(var kvp in LocalDocuments)
			{
				await kvp.Value.LocalClient.StartLocalClientAsync();

				await kvp.Value.LocalClient.AddAsync(new Change()
				{
					Action = ChangeAction.Update,
					Payload = "",
					Owner = "Me",
				});
			}

			;

		}

    }

}
