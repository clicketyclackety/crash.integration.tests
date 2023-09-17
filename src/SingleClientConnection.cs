using System.Text.Json;

using Crash.Changes;
using Crash.Common.Changes;
using Crash.Common.View;
using Crash.Geometry;
using Crash.Handlers.Changes;

namespace integration.tests
{
	public class SingleClientConnection : IntegrationTestBase
	{
		public SingleClientConnection()
		{
			Clients = 1;
		}

		[Test]
		public async Task AddPush()
		{
			await LocalDocuments[0].LocalClient.StartLocalClientAsync();

			var addChange = GeometryChange.CreateChange(Guid.NewGuid(), Users.First().Value, ChangeAction.Add,
				"Example Payload");

			await LocalDocuments[0].LocalClient.PushChangeAsync(addChange);
		}

		[Test]
		public async Task DeletePush()
		{
			await LocalDocuments[0].LocalClient.StartLocalClientAsync();

			var deleteChange = GeometryChange.CreateChange(Guid.NewGuid(), Users.First().Value, ChangeAction.Remove);

			await LocalDocuments[0].LocalClient.PushChangeAsync(deleteChange);
		}

		[Test]
		public async Task DonePush()
		{
			await LocalDocuments[0].LocalClient.StartLocalClientAsync();

			// Do we need to Add?
			ReleaseChangesByUser(0, Users[0]);
		}

		[Test]
		public async Task LockPush()
		{
			await LocalDocuments[0].LocalClient.StartLocalClientAsync();

			var lockchange = GeometryChange.CreateChange(Guid.NewGuid(), Users.First().Value, ChangeAction.Locked);

			await LocalDocuments[0].LocalClient.PushChangeAsync(lockchange);
		}

		[Test]
		public async Task UnlockPush()
		{
			await LocalDocuments[0].LocalClient.StartLocalClientAsync();

			var unlockChange = GeometryChange.CreateChange(Guid.NewGuid(), Users.First().Value, ChangeAction.Unlocked);

			await LocalDocuments[0].LocalClient.PushChangeAsync(unlockChange);
		}

		[Test]
		public async Task UpdatePush()
		{
			await LocalDocuments[0].LocalClient.StartLocalClientAsync();

			var updatePayload = new Dictionary<string, string> { { "Key", "Value" } };
			var payload = JsonSerializer.Serialize(updatePayload);

			var updateChange = GeometryChange.CreateChange(Guid.NewGuid(), Users[0], ChangeAction.Update, payload);

			await LocalDocuments[0].LocalClient.PushChangeAsync(updateChange);
		}

		[Test]
		public async Task CameraChangePush()
		{
			await LocalDocuments[0].LocalClient.StartLocalClientAsync();

			var camera = new Camera(CPoint.Origin, new CPoint(1, 2, 3));
			var cameraChange = CameraChange.CreateChange(camera, Users[0]);

			await LocalDocuments[0].LocalClient.PushChangeAsync(cameraChange);
		}
	}
}
