using Entish.Attributes;

namespace AalSharp.Bars.Data;

[Swappable]
public partial struct AudioResource
{
    public Offset<byte> AmtaOffset;
    public Offset<byte> AssetOffset;
};