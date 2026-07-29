using System.IO.Hashing;
using AalSharp.Bars.Data;
using Entish;

namespace AalSharp.Bars.IO.Data;

public unsafe struct AudioResourcesParts
{
    public Dictionary<ulong, (int Offset, int Alignment, byte[] Data)> Assets;
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
        Dictionary<ulong, (int Offset, int Alignment, byte[] Data)> assets = [];
        
        var headerSize = sizeof(AudioResourcesHeader);
        var hashesSize = sizeof(uint) * bars.Count;
        var resourcesOffset = headerSize + hashesSize;
        var resourcesSize = sizeof(AudioResource) * bars.Count;
        var metadataOffset = resourcesOffset + resourcesSize;
        var metadataSize = bars.Values.Sum(static entry => AudioMetadataParts.GetResSize(entry.Metadata).Total);
        var firstAssetOffset = metadataOffset + metadataSize;
        var assetOffset = firstAssetOffset;

        foreach (var (_, entry) in bars.OrderBy(static entry => entry.Key)) {
            var assetHash = XxHash64.HashToUInt64(entry.Asset);
            if (assets.TryGetValue(assetHash, out var existingAsset) && existingAsset.Data.SequenceEqual(entry.Asset)) {
                continue;
            }

            var alignment = entry.GetAlignment();
            assetOffset = assetOffset.AlignUp(alignment);
            
            assets.Add(assetHash, (assetOffset, alignment, Data: entry.Asset));
            assetOffset += entry.Asset.Length;
        }
        
        var assetSize = assetOffset - firstAssetOffset;

        return new AudioResourcesParts {
            Assets = assets,
            HeaderSize = sizeof(AudioResourcesHeader),
            HashesOffset = headerSize,
            HashesSize = hashesSize,
            ResourcesOffset = resourcesOffset,
            ResourcesSize = resourcesSize,
            MetadataOffset = metadataOffset,
            MetadataSize = metadataSize,
            AssetOffset = firstAssetOffset,
            AssetSize = assetSize,
            Total = firstAssetOffset + assetSize
        };
    }
}