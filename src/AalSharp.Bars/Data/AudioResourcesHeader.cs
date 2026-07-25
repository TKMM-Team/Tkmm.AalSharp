using Entish;
using Entish.Attributes;

namespace AalSharp.Bars.Data;

[Swappable]
public partial struct AudioResourcesHeader()
{
    public const uint AudioResourcesMagic = 0x42415253;
        
    [NeverSwap]
    public readonly uint Magic = AudioResourcesMagic;
    public uint FileSize;
    public Endianness Endianness;
    public ushort Version;
    public int AssetCount;
}