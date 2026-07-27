using Entish.Attributes;

namespace AalSharp.Bars.Data;

[Swappable]
public partial struct AudioMetadataExt()
{
    public const uint AmtaExtMagic = 0x5F545845;

    public readonly uint Magic = AmtaExtMagic;
    public int SectionSize;
    public int NumEntries;
}