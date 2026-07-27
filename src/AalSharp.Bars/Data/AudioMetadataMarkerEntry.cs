using Entish.Attributes;

namespace AalSharp.Bars.Data;

[Swappable]
public partial struct AudioMetadataMarkerEntry
{
    public uint Id;
    public int NameOffset;
    public int StartPos;
    public int Length;
}