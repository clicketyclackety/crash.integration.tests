using System.Text.Json;

using Crash.Changes;
using Crash.Common.Changes;
using Crash.Common.View;
using Crash.Geometry;
using Crash.Server.Hubs;
using Crash.Server.Model;

using Microsoft.EntityFrameworkCore;

using Rhino.UI;

namespace integration.tests
{

	public class Tests : IntegrationTestBase
	{

		public Tests()
		{
			Clients = 1;
		}

		[Test]
		public async Task AddPush()
		{
			await LocalDocuments[0].LocalClient.StartLocalClientAsync();

			var camera = new Camera(CPoint.Origin, new CPoint(1, 2, 3));
			var cameraChange = CameraChange.CreateNew(camera, Users[0]);

			await LocalDocuments[0].LocalClient.AddAsync(cameraChange);
		}

		[Test]
		public async Task DeletePush()
		{
			await LocalDocuments[0].LocalClient.StartLocalClientAsync();

			Guid id = Guid.NewGuid();
			await LocalDocuments[0].LocalClient.DeleteAsync(id);
		}

		[Test]
		public async Task DonePush()
		{
			await LocalDocuments[0].LocalClient.StartLocalClientAsync();

			// Do we need to Add?

			await LocalDocuments[0].LocalClient.DoneAsync(); // Users[0]);
		}

		[Test]
		public async Task LockPush()
		{
			await LocalDocuments[0].LocalClient.StartLocalClientAsync();

			var camera = new Camera(CPoint.Origin, new CPoint(1, 2, 3));
			var cameraChange = CameraChange.CreateNew(camera, Users[0]);

			// await LocalDocuments[0].LocalClient.LockAsync();
		}

		[Test]
		public async Task UnlockPush()
		{
			await LocalDocuments[0].LocalClient.StartLocalClientAsync();

			var camera = new Camera(CPoint.Origin, new CPoint(1, 2, 3));
			var cameraChange = CameraChange.CreateNew(camera, Users[0]);

			// await LocalDocuments[0].LocalClient.UnlockAsync(null);
		}

		[Test]
		public async Task UpdatePush()
		{
			await LocalDocuments[0].LocalClient.StartLocalClientAsync();

			var camera = new Camera(CPoint.Origin, new CPoint(1, 2, 3));
			var cameraChange = CameraChange.CreateNew(camera, Users[0]);

			var updatePayload = new Dictionary<string, string>
			{
				{ "Key", "Value" }
			};

			Change updateChange = new()
			{
				Action = ChangeAction.Update,
				Owner = Users[0],
				Payload = JsonSerializer.Serialize(updatePayload),
				Type = "Camera",
			};

			await LocalDocuments[0].LocalClient.UpdateAsync(null);
		}

		[Test]
		public async Task CameraChangePush()
		{
			await LocalDocuments[0].LocalClient.StartLocalClientAsync();

			var camera = new Camera(CPoint.Origin, new CPoint(1, 2, 3));
			var cameraChange = CameraChange.CreateNew(camera, Users[0]);

			await LocalDocuments[0].LocalClient.CameraChangeAsync(null);
		}

	}

}
