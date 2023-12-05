using System.Collections.Concurrent;

using Crash.Changes;
using Crash.Handlers.Changes;

namespace integration.tests
{
	public class MaximumClientTests : IntegrationTestBase
	{
		private static readonly int clientCount = 1000;

		public MaximumClientTests()
		{
			Clients = clientCount;
		}

		[Test]
		public async Task MaximumConnectionsTest()
		{
			var recieved = new ConcurrentBag<Change>();

			foreach (var kvp in LocalDocuments)
			{
				await kvp.Value.LocalClient.StartLocalClientAsync();

				kvp.Value.LocalClient.OnPushChange += change =>
				{
					recieved.Add(change);
				};
			}

			LocalDocuments[0].LocalClient
				.PushChangeAsync(GeometryChange.CreateChange(Guid.NewGuid(), Users[0], ChangeAction.Add,
					"Example Payload!"));

			var delay = 10;
			Assert.That(() => recieved.Count, Is.EqualTo(clientCount - 1).After(delay).Seconds.PollEvery(250));
		}
	}
}
