using sNet;

namespace sNetServer;

internal static class Program
{
	private static void Main()
	{
		Logger.Initialize();

		try
		{
			if (!new Server().Run())
			{
				Console.Read();
			}
		}
		catch (Exception ex)
		{
			Logger.Error(ex.Message);
			Console.Read();
		}
		finally
		{
			Logger.Close();
		}
	}
}