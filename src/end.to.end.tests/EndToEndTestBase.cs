using Crash.Common.Document;
using Crash.Handlers;
using Crash.Handlers.Plugins;
using Crash.Handlers.Plugins.Geometry;
using Crash.Server;
using Crash.Server.Model;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using Rhino;

namespace end.to.end.tests
{
	public abstract class EndToEndTestBase
	{
		private const string ServerUrl = "http://0.0.0.0:8080";
		private const string ClientUrl = "http://localhost:8080/Crash";

		protected WebApplication App;
		protected CrashContext CurrentContext;

		private CrashDoc crashDoc { get; set; }
		private string UserName => "Clicky";

		[SetUp]
		public void Setup()
		{
			crashDoc = CrashDocRegistry.CreateAndRegisterDocument(RhinoDoc.ActiveDoc);
			var dispatcher = crashDoc.Dispatcher as EventDispatcher;
			dispatcher.RegisterDefaultServerNotifiers();
			dispatcher.RegisterDefinition(new GeometryChangeDefinition());

			crashDoc.LocalClient.RegisterConnection(UserName, new Uri(ClientUrl));

			var App = Program.CreateApplication($"--urls {ServerUrl}");
			App.RunAsync();
			CurrentContext = App.Services.CreateScope().ServiceProvider.GetService<CrashContext>();
		}

		[TearDown]
		public async Task TearDown()
		{
			await CrashDocRegistry.DisposeOfDocumentAsync(crashDoc);
			CurrentContext = null;
			await App.StopAsync();
		}
	}
}
