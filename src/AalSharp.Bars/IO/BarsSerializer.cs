using System.IO.Hashing;
using System.Runtime.InteropServices;
using AalSharp.Amta.IO;
using AalSharp.Bars.Data;
using AalSharp.Bars.IO.Data;
using AalSharp.Helpers;
using Entish;

namespace AalSharp.Bars.IO;

public static class BarsSerializer
{
    public static byte[] Serialize(AudioResource bars, Endianness endianness = Endianness.Little)
    {
        var size = new ResAudioResourceSize(bars);
        var buffer = new byte[size.Total];
        Serialize(bars, buffer, size, endianness);
        return buffer;
    }

    public static unsafe void Serialize(AudioResource bars, Span<byte> span, ResAudioResourceSize size, Endianness endianness = Endianness.Little)
    {
        fixed (byte* ptr = span) {
            Serialize(bars, (ResAudioResource*)ptr, size, endianness);
        }
    }

    public static unsafe void Serialize(AudioResource bars, ResAudioResource* resAudioResource, ResAudioResourceSize size, Endianness endianness = Endianness.Little)
    {
        *resAudioResource = new ResAudioResource {
            FileSize = (uint)size.Total,
            AssetCount = bars.Count,
            Endianness = endianness
        };

        var hashes = (uint*)(resAudioResource + 1);
        foreach (uint hash in bars.Keys.Order()) {
            *hashes = hash;
            hashes++;
        }

        var resources = (ResAssetOffset*)hashes;
        var metadata = (byte*)resAudioResource + size.MetadataOffset;

        foreach (var (_, entry) in bars.OrderBy(static entry => entry.Key)) {
            var assetHash = XxHash64.HashToUInt64(entry.Asset);
            var asset = size.Assets[assetHash];

            *resources = new ResAssetOffset {
                AmtaOffset = new Offset<byte>(size.MetadataOffset),
                AssetOffset = new Offset<byte>(asset.Offset),
            };

            var metadataSize = entry.Metadata.GetResSize();
            entry.Metadata.GetSerializer().Serialize(entry.Metadata, metadata, metadataSize, endianness);

            resources++;
            metadata += metadataSize.Total;
            size.MetadataOffset += metadataSize.Total;

            size.AssetOffset = size.AssetOffset.AlignUp(asset.Alignment);
            if (asset.Data is null || asset.Offset != size.AssetOffset) {
                continue;
            }

            Marshal.Copy(asset.Data, 0, (IntPtr)resAudioResource + asset.Offset, asset.Data.Length);
            size.AssetOffset += asset.Data.Length;
        }

        SwapEndiannessFromSystem(resAudioResource);
    }

    public static unsafe AudioResource Deserialize<TAmtaSerializer>(void* resAudioResource, out Endianness endianness)
        where TAmtaSerializer : IAmtaSerializer
    {
        AudioResource bars = [];

        var audioResources = (ResAudioResource*)resAudioResource;
        endianness = EndianUtils.GetTrueEndianness(audioResources->Endianness);

        SwapEndianness(audioResources);

        var resCount = audioResources->AssetCount;
        var hashes = (uint*)((byte*)audioResources + sizeof(ResAudioResource));
        var resources = (ResAssetOffset*)((byte*)hashes + sizeof(uint) * resCount);

        for (int i = 0; i < resCount; i++) {
            var hash = hashes[i];
            var resource = resources[i];
            var metadata = resource.AmtaOffset.GetPointer(resAudioResource);
            var asset = resource.AssetOffset.GetPointer(resAudioResource);

            bars[hash] = new AudioResourceAsset {
                Metadata = TAmtaSerializer.Deserialize(metadata, out _),
                Asset = asset is null ? null : new ReadOnlySpan<byte>(asset, ResourceHelper.GetAssetSize(asset)).ToArray()
            };
        }

        return bars;
    }

    public static unsafe void SwapEndianness(ResAudioResource* resAudioResource)
    {
        if (!EndianUtils.ShouldSwap(resAudioResource->Endianness)) {
            return;
        }

        ResAudioResource.Swap(resAudioResource);

        var resCount = resAudioResource->AssetCount;
        byte* pos = (byte*)resAudioResource + sizeof(ResAudioResource);

        for (int i = 0; i < resCount; i++) {
            EndianUtils.Swap((uint*)pos);
            pos += sizeof(uint);
        }

        for (int i = 0; i < resCount; i++) {
            ResAssetOffset.Swap((ResAssetOffset*)pos);
            pos += sizeof(ResAssetOffset);
        }
    }

    /// <summary>
    /// Swap the endianness from a system-matching endianness (backwards for serialization)
    /// </summary>
    public static unsafe void SwapEndiannessFromSystem(ResAudioResource* resAudioResource)
    {
        if (EndianUtils.ShouldSwap(resAudioResource->Endianness)) {
            EndianUtils.Swap((ushort*)resAudioResource + 4);
            return;
        }

        var resCount = resAudioResource->AssetCount;
        byte* pos = (byte*)resAudioResource + sizeof(ResAudioResource);

        for (int i = 0; i < resCount; i++) {
            EndianUtils.Swap((uint*)pos);
            pos += sizeof(uint);
        }

        for (int i = 0; i < resCount; i++) {
            ResAssetOffset.Swap((ResAssetOffset*)pos);
            pos += sizeof(ResAssetOffset);
        }

        ResAudioResource.Swap(resAudioResource);
    }
}