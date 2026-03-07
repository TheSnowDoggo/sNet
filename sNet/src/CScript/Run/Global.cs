using System.Collections.Frozen;

namespace sNet.CScriptPro;

public static class Global
{
    public static readonly FrozenDictionary<string, Obj> Exports = new Dictionary<string, Obj>()
    {
        { "Vec2", Vec2Obj.Export },
        { "Part", Part.Export },
        { "Uid", UidObj.Export },
        { "Event", Event.Export },
    }.ToFrozenDictionary();
}