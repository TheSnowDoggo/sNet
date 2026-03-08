using sNet;

namespace sNetClient;

internal static class Program
{
	private static void Main()
	{
		Logger.Initialize();

		try
		{
			Console.CursorVisible = false;

			Client.Instance.Run();
		}
		catch (Exception ex)
		{
			Logger.Error(ex.ToString());
			Console.Read();
		}
		finally
		{
			Directory.Delete("assets", true);
			Logger.Close();
		}
	}
}