using Entish;
using Entish.Attributes;

namespace AalSharp.Bars.Data;

[Swappable]
public partial struct AudioResourcesHeader()
{
    [NeverSwap]
    public readonly uint Magic = AudioResources.Magic;
    public uint FileSize;
    public Endianness Endianness;
    public ushort Version = AudioResources.Version;
    public int AssetCount;
}