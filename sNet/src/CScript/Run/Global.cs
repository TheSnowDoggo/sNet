using System.Collections.Frozen;

namespace sNet.CScriptPro;

public static class Global
{
    public static readonly FrozenDictionary<string, CObj> Exports = new Dictionary<string, CObj>()
    {
        { "Vec2", CVec2.Export },
        { "Part", Part.Export },
        { "Uid", CUid.Export },
    }.ToFrozenDictionary();
}