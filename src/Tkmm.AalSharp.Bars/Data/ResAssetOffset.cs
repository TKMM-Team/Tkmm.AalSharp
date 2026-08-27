using Entish.Attributes;

namespace Tkmm.AalSharp.Bars.Data;

[Swappable]
public partial struct ResAssetOffset
{
    public Offset<byte> AmtaOffset;
    public Offset<byte> AssetOffset;
};