using AalSharp.Bars.Data;
using Entish;

namespace AalSharp.Bars.IO.Data;

public unsafe struct AudioResourcesParts
{
    public int HeaderSize;
    public int HashesOffset;
    public int HashesSize;
    public int ResourcesOffset;
    public int ResourcesSize;
    public int MetadataOffset;
    public int MetadataSize;
    public int AssetOffset;
    public int AssetSize;
    public int Total;

    public static AudioResourcesParts GetResSize(BarsFile bars)
    {
        var headerSize = sizeof(AudioResourcesHeader);
        var hashesSize = sizeof(uint) * bars.Count;
        var resourcesOffset = headerSize + hashesSize;
        var resourcesSize = sizeof(AudioResource) * bars.Count;
        var metadataOffset = resourcesOffset + resourcesSize;
        var metadataSize = bars.Values.Sum(static entry => AudioMetadataParts.GetResSize(entry.Metadata).Total);
        var assetOffset = metadataOffset + metadataSize.AlignUp(0x8);
        var assetSize = bars.Values.Sum(static entry => entry.Asset.Length.AlignUp(BarsSerializer.AssetAlignment));

        return new AudioResourcesParts {
            HeaderSize = sizeof(AudioResourcesHeader),
            HashesOffset = headerSize,
            HashesSize = hashesSize,
            ResourcesOffset = resourcesOffset,
            ResourcesSize = resourcesSize,
            MetadataOffset = metadataOffset,
            MetadataSize = metadataSize,
            AssetOffset = assetOffset,
            AssetSize = assetSize,
            Total = assetOffset + assetSize
        };
    }
}