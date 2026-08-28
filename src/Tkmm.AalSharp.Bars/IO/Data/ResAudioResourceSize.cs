using System.IO.Hashing;
using Tkmm.AalSharp.Bars.Data;
using Entish;

namespace Tkmm.AalSharp.Bars.IO.Data;

public unsafe struct ResAudioResourceSize : IResourceSize
{
    public readonly Dictionary<ulong, (int Offset, int Alignment, byte[]? Data)> Assets;
    public readonly int HeaderSize;
    public readonly int HashesOffset;
    public readonly int HashesSize;
    public readonly int ResourcesOffset;
    public readonly int ResourcesSize;
    public int MetadataOffset;
    public readonly int MetadataSize;
    public int AssetOffset;
    public readonly int AssetSize;

    public int Total { get; private init; }

    public ResAudioResourceSize(AudioResource bars)
    {
        Assets = [];
        HeaderSize = sizeof(ResAudioResource);
        HashesOffset = HeaderSize;
        HashesSize = sizeof(uint) * bars.Count;
        ResourcesOffset = HeaderSize + HashesSize;
        ResourcesSize = sizeof(ResAssetOffset) * bars.Count;
        MetadataOffset = ResourcesOffset + ResourcesSize;
        MetadataSize = bars.Values.Sum(static entry => entry.Metadata.Length);
        var firstAssetOffset = (MetadataOffset + MetadataSize).AlignUp(0x40);
        AssetOffset = firstAssetOffset;

        foreach (var (_, entry) in bars.OrderBy(static entry => entry.Key)) {
            var assetHash = XxHash64.HashToUInt64(entry.Asset);
            if (Assets.TryGetValue(assetHash, out var existingAsset) && existingAsset.Data.SequenceEqual(entry.Asset)) {
                continue;
            }

            var alignment = entry.GetAlignment();
            AssetOffset = AssetOffset.AlignUp(alignment);

            Assets.Add(assetHash, (Offset: entry.Asset is null ? -1 : AssetOffset, alignment, Data: entry.Asset));
            AssetOffset += entry.Asset?.Length ?? 0;
        }

        AssetSize = AssetOffset - firstAssetOffset;
        AssetOffset = firstAssetOffset;

        Total = AssetOffset + AssetSize;
    }
}