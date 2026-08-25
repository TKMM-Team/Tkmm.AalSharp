using Entish.Attributes;

namespace AalSharp.Amta.Data;

[Swappable]
public partial struct ResMarker
{
    public uint Id;
    public int NameOffset;
    public int StartPos;
    public int Length;
}