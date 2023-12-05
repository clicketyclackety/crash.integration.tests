using System.Text.Json;

using Crash.Changes;
using Crash.Geometry;

using Rhino;
using Rhino.Geometry;
using Rhino.Runtime;

namespace end.to.end.tests
{
	public sealed class EventWrappingTests : EndToEndTestBase
	{
		[Test]
		public void TestAdd()
		{
			Assert.That(CurrentContext.LatestChanges, Is.Empty);
			RhinoApp.RunScript("-Box 0,0,0 100,100,0 100", true);
			Assert.That(CurrentContext.LatestChanges.Count(), Is.EqualTo(1));

			var currentChange = CurrentContext.LatestChanges.FirstOrDefault();
			Assert.That(currentChange.Action.HasFlag(ChangeAction.Add), Is.True);
			Assert.That(currentChange.Action.HasFlag(ChangeAction.Transform), Is.True);
			Assert.That(currentChange.Action.HasFlag(ChangeAction.Update), Is.True);
			Assert.That(currentChange.Action.HasFlag(ChangeAction.Temporary), Is.True);
			Assert.That(currentChange.Payload, Is.Not.Empty);

			var payload = JsonSerializer.Deserialize<PayloadPacket>(currentChange.Payload);
			Assert.That(payload.Transform, Is.EqualTo(CTransform.Unset));
			Assert.That(payload.Updates, Is.Empty);

			var boxBrep = CommonObject.FromJSON(payload.Data) as Brep;
			var createdBox = new Box(new BoundingBox(0, 0, 0, 100, 100, 100));
			Assert.That(boxBrep.GetVolume(1, 1), Is.EqualTo(createdBox.Volume));
			// TODO : Assert
		}
	}
}
