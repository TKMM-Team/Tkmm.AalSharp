using Entish.Attributes;

namespace AalSharp.Bars.Data;

[Swappable]
public partial struct AudioMetadataMarker()
{
    public const uint AmtaMarkerMagic = 0x4B52414D;

    [NeverSwap]
    public readonly uint Magic = AmtaMarkerMagic;
    public int SectionSize;
    public int NumEntries;
}