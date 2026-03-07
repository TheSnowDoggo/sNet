namespace sNet;

public static class FileUtils
{
	public static IEnumerable<string> EnumerateFilesRecursive(string rootDirectory)
	{
		var queue = new Queue<string>();
		queue.Enqueue(rootDirectory);

		while (queue.TryDequeue(out string directory))
		{
			foreach (var entry in Directory.EnumerateFileSystemEntries(directory))
			{
				var attr = File.GetAttributes(entry);

				if ((attr & FileAttributes.Directory) != 0)
				{
					queue.Enqueue(entry);
					continue;
				}

				yield return entry;
			}
		}
	}
}