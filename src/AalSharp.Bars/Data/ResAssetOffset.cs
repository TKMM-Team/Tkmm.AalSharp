using Entish.Attributes;

namespace AalSharp.Bars.Data;

[Swappable]
public partial struct ResAssetOffset
{
    public Offset<byte> AmtaOffset;
    public Offset<byte> AssetOffset;
};