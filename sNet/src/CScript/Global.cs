using System.Collections.Frozen;
using SCENeo;
using SCENeo.Input;

namespace sNet.CScriptPro;

public static class Global
{
    public static readonly FrozenDictionary<string, Obj> Exports = new Dictionary<string, Obj>()
    {
        { "Vec2", Vec2Obj.Export },
        { "Part", Part.Export },
        { "Uid", UidObj.Export },
        { "Event", Event.Export },
        { "Key", new EnumTable<Key>() },
        { "Color", new EnumTable<SCEColor>() },
    }.ToFrozenDictionary();
}