using System.Collections.Frozen;

namespace sNet.CScriptPro;

public sealed class PackageManager
{
	public PackageManager()
	{
	}
	
	public PackageManager(Dictionary<string, Package> packages)
	{
		Packages = packages;
	}
	
	public static PackageManager Default { get; } = new Dictionary<string, Package>()
	{
		{ "IO", new IOPackage() },
		{ "Math", new MathPackage() },
		{ "Time", new TimePackage() },
	};

	public static PackageManager Current { get; set; } = Default;
	
	public Dictionary<string, Package> Packages { get; init; } = [];

	public static implicit operator PackageManager(Dictionary<string, Package> packages)
		=> new PackageManager(packages);

	public void AddDefault(Package package)
	{
		Packages.Add(package.Name, package);
	}
}