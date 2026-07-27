using Entish;
using Entish.Attributes;

namespace AalSharp.Bars.Data;

[Swappable]
public partial struct AudioMetadataHeader()
{
    [NeverSwap]
    public readonly uint Magic = AudioMetadata.Magic;
    public Endianness Endianness;
    public ushort Version = 0x400;
    public int FileSize;
}