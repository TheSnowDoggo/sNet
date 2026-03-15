using System.Collections.Frozen;
using SCENeo.Input;
using SCEWin.Input;
using sNet.CScriptPro;

namespace sNetClient;

public sealed class InputPackage : Package
{
	public static readonly Event InputEvent = new Event();
	
	public static readonly ReadOnlyTable Exports = new Dictionary<Obj, Obj>()
	{
		{ "keyPressed", GlobalFunction.Create(KeyPressed, TypeId.Number) },
		{ "isFocused", GlobalFunction.Create(IsFocused) },
		{ "input", InputEvent },
	}.ToFrozenDictionary();
	
	public override string Name => "Input";
	public override ReadOnlyTable Export => Exports;
	
	private static Bool KeyPressed(Obj[] args)
	{
		var vkCode = (int)args[0];

		if (Environment.OSVersion.Platform != PlatformID.Win32NT)
		{
			throw new InvalidOperationException($"Key press is not available on platform {Environment.OSVersion.Platform}.");
		}

		if (vkCode is < 0 or >= 256)
		{
			return false;
		}
		
		#pragma warning disable CA1416
		return Input.KeyPressed((Key)vkCode);
		#pragma warning restore CA1416
	}

	private static Bool IsFocused(Obj[] args)
	{
		if (Client.Instance.Scenes.InputFocus != null)
		{
			return false;
		}
		
		if (Environment.OSVersion.Platform != PlatformID.Win32NT)
		{
			return true;
		}

#if DEBUG
		return true;
#else
		return Input.InFocus();
#endif
	}
}