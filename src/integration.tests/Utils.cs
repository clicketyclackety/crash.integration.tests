namespace integration.tests
{
	public static class Utils
	{
		public static void AssertWithPolling(Func<bool> valueCheck, int delay = 5)
		{
			Assert.That(() => valueCheck(), Is.True.After(delay).Seconds.PollEvery(250).MilliSeconds);
		}
	}
}
