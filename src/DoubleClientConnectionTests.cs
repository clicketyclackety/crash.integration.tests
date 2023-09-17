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
			await PushNewChangeToServer(0, changeId, Users[0]);

			// Transform Change
			await TransformExistingChangeOnServer(0, changeId, Users[0]);

			// Add some Update Values to Change
			await AppendUpdateToExistingChangeOnServer(0, changeId, Users[0]);

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
			// Connect 2 Clients
			await LocalDocuments[0].LocalClient.StartLocalClientAsync();
			await LocalDocuments[1].LocalClient.StartLocalClientAsync();

			var changeId = Guid.NewGuid();

			// Add with 1st
			await PushNewChangeToServer(0, changeId, Users[0]);

			// Release
			await ReleaseChangesByUser(0, Users[0]);

			// Transform
			await TransformExistingChangeOnServer(0, changeId, Users[0]);

			// Release
			await ReleaseChangesByUser(0, Users[0]);

			// Update
			// Lock
			// Unlock
		}

		/// <summary>Tests that release selected works as advertised</summary>
		[Test]
		public async Task TestRelease()
		{
			var clientCount = 3;

			// Connect 3 clients
			for (var i = 0; i < clientCount; i++)
			{
				await LocalDocuments[i].LocalClient.StartLocalClientAsync();
			}

			// Add items on each client (x3)
			for (var i = 0; i < clientCount; i++)
			{
				await PushNewChangeToServer(i, Guid.NewGuid(), Users[i]);
			}

			var firstClientRecievedRelease = false;
			var secondClientRecievedRelease = false;
			var thirdClientRecievedRelease = false;

			LocalDocuments[0].LocalClient.OnPushChange += changes =>
			{
				firstClientRecievedRelease = true;
			};
			LocalDocuments[1].LocalClient.OnPushChange += changes =>
			{
				secondClientRecievedRelease = true;
			};
			LocalDocuments[2].LocalClient.OnPushChange += changes =>
			{
				thirdClientRecievedRelease = true;
			};

			// release all from 1st, then 2nd, then 3rd in a cycle
			for (var i = 0; i < clientCount; i++)
			{
				await ReleaseChangesByUser(i, Users[i]);
			}

			// assert releases all work correctly
			Assert.That(() => firstClientRecievedRelease, Is.True.After(3).Seconds.PollEvery(250));
			Assert.That(() => secondClientRecievedRelease, Is.True.After(3).Seconds.PollEvery(250));
			Assert.That(() => thirdClientRecievedRelease, Is.True.After(3).Seconds.PollEvery(250));
		}

		/// <summary>Tests that release selected works as advertised</summary>
		[Test]
		public async Task TestReleaseSelected()
		{
			// Connect 2 clients
			await LocalDocuments[0].LocalClient.StartLocalClientAsync();
			await LocalDocuments[1].LocalClient.StartLocalClientAsync();

			// Add (5 items) with 1st
			for (var i = 0; i < 5; i++)
			{
				await PushNewChangeToServer(0, Guid.NewGuid(), Users[0]);
			}

			// release 3 from 1st
			Assert.True(false, "test not written yet");

			// assert
			//	- 3 are released on #1
			//	- 3 are not temp, 2 are temp on #2
			//	- Server has them stored correctly
		}
	}
}
