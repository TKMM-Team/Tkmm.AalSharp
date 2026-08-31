using System.IO.Hashing;
using System.Runtime.InteropServices;
using Entish;
using Tkmm.AalSharp.Bars.Data;
using Tkmm.AalSharp.Bars.IO.Data;
using Tkmm.AalSharp.Helpers;

namespace Tkmm.AalSharp.Bars.IO;

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
            FileSize = size.Total,
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
        
        var publicHashCount = resAudioResource->PublicHashesCount;
        *publicHashCount = 0;
        
        var publicHashes = (uint*)publicHashCount + 1;

        foreach (var (hash, entry) in bars.OrderBy(static entry => entry.Key)) {
            var assetHash = XxHash64.HashToUInt64(entry.Asset);
            var asset = size.Assets[assetHash];

            if (entry.IsPublic) {
                publicHashes[*publicHashCount] = hash; 
                (*publicHashCount)++;
            }

            *resources = new ResAssetOffset {
                AmtaOffset = new Offset<byte>(size.MetadataOffset),
                AssetOffset = new Offset<byte>(asset.Offset),
            };

            Marshal.Copy(entry.Metadata, 0, (IntPtr)metadata, entry.Metadata.Length);

            resources++;
            metadata += entry.Metadata.Length;
            size.MetadataOffset += entry.Metadata.Length;

            size.AssetOffset = size.AssetOffset.AlignUp(asset.Alignment);
            if (asset.Data is null || asset.Offset != size.AssetOffset) {
                continue;
            }

            Marshal.Copy(asset.Data, 0, (IntPtr)resAudioResource + asset.Offset, asset.Data.Length);
            size.AssetOffset += asset.Data.Length;
        }

        SwapEndiannessFromSystem(resAudioResource);
    }

    public static unsafe AudioResource Deserialize(void* resAudioResource, out Endianness endianness)
    {
        AudioResource bars = [];

        var audioResources = (ResAudioResource*)resAudioResource;
        endianness = EndianUtils.GetTrueEndianness(audioResources->Endianness);

        SwapEndianness(audioResources);

        var resCount = audioResources->AssetCount;
        var hashes = (uint*)((byte*)audioResources + sizeof(ResAudioResource));
        var resources = (ResAssetOffset*)((byte*)hashes + sizeof(uint) * resCount);
        var publicHashes = audioResources->GetPublicHashes();

        for (int i = 0; i < resCount; i++) {
            var hash = hashes[i];
            var resource = resources[i];
            var metadata = resource.AmtaOffset.GetPointer(resAudioResource);
            var asset = resource.AssetOffset.GetPointer(resAudioResource);

            Console.WriteLine((int)(metadata - (byte*)resAudioResource));

            var amta = (ResAudioMetadataHeader*)metadata;
            var metadataBufferSize = ((ResAudioMetadataHeader*)metadata)->FileSize;
            var assetSize = resource.AssetOffset.Value > 0 ? GetResourceSize(audioResources, resources, i) : 0;

            bars[hash] = new AudioResourceAsset {
                Metadata = [.. new ReadOnlySpan<byte>(metadata, metadataBufferSize)],
                Asset = asset is null ? null : new ReadOnlySpan<byte>(asset, assetSize).ToArray(),
                IsPublic = publicHashes.Contains(hash)
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

    private static unsafe int GetResourceSize(ResAudioResource* resAudioResource, ResAssetOffset* resAssetOffsets, int idx)
    {
        var res = resAssetOffsets[idx];

        if (idx == 0) {
            goto NextOrEnd;
        }

        var prev = resAssetOffsets[idx - 1];

        // When the previous offset is larger the
        // target offset refers to a previous file
        if (prev.AssetOffset.Value > res.AssetOffset.Value) {
            // Locate the first occurrence of the offset
            for (int i = 0; i < idx; i++) {
                var firstOccurrence = resAssetOffsets[i];
                if (firstOccurrence.AssetOffset.Value == res.AssetOffset.Value) {
                    res = firstOccurrence;
                    idx = i;
                    goto NextOrEnd;
                }
            }

            throw new InvalidDataException(
                $"Could not locate an earlier occurrence of the offset 0x{res.AssetOffset.Value:x8} @ [{idx}]");
        }

    NextOrEnd:
        bool isEnd;
        
        // Assets are written in order, so we can always
        // skip to the offset of the next written asset (or EOF) 
        while (!(isEnd = ++idx >= resAudioResource->AssetCount) &&
               res.AssetOffset.Value >= resAssetOffsets[idx].AssetOffset.Value) {
        }

        var nextOrEnd = isEnd
            ? resAudioResource->FileSize
            : resAssetOffsets[idx].AssetOffset.Value;

        if (res.AssetOffset.Value > nextOrEnd) {
            throw new InvalidDataException(
                $"Asset offset {res.AssetOffset.Value:x8} exceeds end of file");
        }

        return nextOrEnd - res.AssetOffset.Value;
    }
}