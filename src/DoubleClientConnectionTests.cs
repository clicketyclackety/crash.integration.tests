using System.Text.Json;

using Crash.Changes;
using Crash.Changes.Utils;
using Crash.Common.Changes;
using Crash.Common.View;
using Crash.Geometry;
using Crash.Handlers.Changes;
using Crash.Server.Model;

using Microsoft.Extensions.DependencyInjection;

namespace integration.tests
{
	public class DoubleClientConnectionTests : IntegrationTestBase
	{
		public DoubleClientConnectionTests()
		{
			Clients = 3;
		}

		[Test]
		public async Task CameraChangeTest()
		{
			foreach (var kvp in LocalDocuments)
			{
				await kvp.Value.LocalClient.StartLocalClientAsync();
			}

			var camera = new Camera(CPoint.Origin, new CPoint(1, 2, 3));
			var cameraChange = CameraChange.CreateNew(camera, Users[0]);

			var Client1Recieved = false;
			var Client2Recieved = false;

			LocalDocuments[1].LocalClient.OnPushChange += changes =>
			{
				Client1Recieved = true;
				Assert.That(changes, Is.Not.Empty);
			};
			LocalDocuments[2].LocalClient.OnPushChange += changes =>
			{
				Client2Recieved = true;
				Assert.That(changes, Is.Not.Empty);
			};

			await LocalDocuments[0].LocalClient.PushChangeAsync(new Change(cameraChange));

			var i = 0;
			while (i < 100)
			{
				Thread.Sleep(250);
				if (Client1Recieved && Client2Recieved)
				{
					return;
				}

				i++;
			}

			;
		}

		[Test]
		public async Task ChangeLifetime()
		{
			// Connect 1 Client
			await LocalDocuments[0].LocalClient.StartLocalClientAsync();

			var changeId = Guid.NewGuid();

			// Add a piece of Geometry
			await LocalDocuments[0].LocalClient.PushChangeAsync(
				GeometryChange.CreateChange(changeId, Users[0],
					ChangeAction.Add | ChangeAction.Temporary,
					"Test!"
				));

			// Transform Change
			var transformPayload = JsonSerializer.Serialize(new CTransform(Enumerable.Repeat(100.0, 16).ToArray()));
			await LocalDocuments[0].LocalClient.PushChangeAsync(
				GeometryChange.CreateChange(changeId, Users[0],
					ChangeAction.Transform,
					transformPayload
				));

			// Add some Update Values to Change
			var updateData = new Dictionary<string, string> { { "Key", "Value" } };
			var updatePayload = JsonSerializer.Serialize(updateData);
			await LocalDocuments[0].LocalClient.PushChangeAsync(
				GeometryChange.CreateChange(changeId, Users[0],
					ChangeAction.Update,
					updatePayload
				));

			Change triageChange = null;

			// Connect a 2nd client
			LocalDocuments[1].LocalClient.OnInitializeChanges += changes =>
			{
				// Monitor Init
				// Collect Change with data triage!
				triageChange = changes.FirstOrDefault(c => c.Id == changeId);
				Assert.That(changes, Is.Not.Empty);
			};

			var scope = App.Services.CreateScope();
			var con = scope.ServiceProvider.GetService<CrashContext>();

			await LocalDocuments[1].LocalClient.StartLocalClientAsync();

			Assert.That(() =>
			{
				if (triageChange is null)
				{
					return false;
				}

				Assert.That(triageChange.Action.HasFlag(ChangeAction.Add));
				Assert.That(triageChange.Action.HasFlag(ChangeAction.Update));
				Assert.That(triageChange.Action.HasFlag(ChangeAction.Transform));

				Assert.That(PayloadUtils.TryGetPayloadFromChange(triageChange, out var packet));

				Assert.That(packet.Transform, Is.Not.EqualTo(CTransform.Unset));
				Assert.That(packet.Data, Is.Not.Null.Or.Empty);
				Assert.That(packet.Updates, Is.Not.Empty);

				return true;
			}, Is.True.After(2).Seconds.PollEvery(250));
		}

		/// <summary>Testing that a single Change type is transmitted</summary>
		[Test]
		public async Task TestChangeTransmittedToOtherClient() // Delete // Update // Transform
		{
			// Connect 2 Clients
			await LocalDocuments[0].LocalClient.StartLocalClientAsync();
			await LocalDocuments[1].LocalClient.StartLocalClientAsync();

			var secondClientRecieved = false;

			LocalDocuments[1].LocalClient.OnPushChange += change =>
			{
				secondClientRecieved = true;
			};

			// Add with 1st client
			await LocalDocuments[0].LocalClient.PushChangeAsync(
				GeometryChange.CreateChange(Guid.NewGuid(), Users[0],
					ChangeAction.Add | ChangeAction.Temporary,
					"Test!"
				));

			// Check for recieve of Add
			Utils.AssertWithPolling(() => secondClientRecieved);
		}

		/// <summary>Tests that a base Change is modified correctly</summary>
		[Test]
		public async Task TestApplied() // Update & transform & Lock & Unlock
		{
			// Connect 2 clients
			// Add with 1st
			// Release
			// Transform
			// Release
			// Update
			// Lock
			// Unlock
		}

		/// <summary>Tests that release selected works as advertised</summary>
		[Test]
		public async Task TestRelease()
		{
			// Connect 3 clients
			// Add items on each client (x3)
			// release all from 1st, then 2nd, then 3rd in a cycle
			// assert releases all work correctly
		}

		/// <summary>Tests that release selected works as advertised</summary>
		[Test]
		public async Task TestReleaseSelected()
		{
			// Connect 2 clients
			// Add (5 items) with 1st
			// release 3 from 1st
			// assert
			//	- 3 are released on #1
			//	- 3 are not temp, 2 are temp on #2
			//	- Server has them stored correctly
		}
	}
}
