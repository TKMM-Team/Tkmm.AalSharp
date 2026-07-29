using System.IO.Hashing;
using System.Runtime.InteropServices;
using AalSharp.Bars.Data;
using AalSharp.Bars.IO.Data;
using AalSharp.Helpers;
using Entish;

namespace AalSharp.Bars.IO;

public static class BarsSerializer
{
    public static byte[] Serialize(BarsFile bars, Endianness endianness = Endianness.Little)
    {
        var size = AudioResourcesParts.GetResSize(bars);
        var buffer = new byte[size.Total];
        Serialize(bars, buffer, size, endianness);
        return buffer;
    }

    public static unsafe void Serialize(BarsFile bars, Span<byte> span, AudioResourcesParts size, Endianness endianness = Endianness.Little)
    {
        fixed (byte* ptr = span) {
            Serialize(bars, (AudioResources*)ptr, size, endianness);
        }
    }

    public static unsafe void Serialize(BarsFile bars, AudioResources* resAudioResources, AudioResourcesParts size, Endianness endianness = Endianness.Little)
    {
        *resAudioResources = new AudioResources {
            Header = new AudioResourcesHeader {
                FileSize = (uint)size.Total,
                AssetCount = bars.Count,
                Endianness = endianness
            }
        };

        var hashes = (uint*)(resAudioResources + 1);
        foreach (uint hash in bars.Keys.Order()) {
            *hashes = hash;
            hashes++;
        }

        var resources = (AudioResource*)hashes;
        var metadata = (AudioMetadata*)((byte*)resAudioResources + size.MetadataOffset);

        foreach (var (_, entry) in bars.OrderBy(static entry => entry.Key)) {
            var assetHash = XxHash64.HashToUInt64(entry.Asset);
            var asset = size.Assets[assetHash];

            *resources = new AudioResource {
                AmtaOffset = new Offset<byte>((uint)size.MetadataOffset),
                AssetOffset = new Offset<byte>((uint)asset.Offset),
            };

            var metadataSize = AudioMetadataParts.GetResSize(entry.Metadata);
            AmtaSerializer.Serialize(entry.Metadata, metadata, metadataSize, endianness);

            resources++;
            metadata = (AudioMetadata*)((byte*)metadata + metadataSize.Total);
            size.MetadataOffset += metadataSize.Total;

            size.AssetOffset = size.AssetOffset.AlignUp(asset.Alignment);
            if (asset.Offset != size.AssetOffset) {
                continue;
            }

            Marshal.Copy(entry.Asset, 0, (IntPtr)resAudioResources + asset.Offset, entry.Asset.Length);
            size.AssetOffset += entry.Asset.Length;
        }

        SwapEndiannessFromSystem(resAudioResources);
    }

    public static unsafe BarsFile Deserialize(void* resAudioResources, out Endianness endianness)
    {
        BarsFile bars = [];

        var audioResources = (AudioResources*)resAudioResources;
        endianness = EndianUtils.GetTrueEndianness(audioResources->Header.Endianness);

        SwapEndianness(audioResources);

        var resCount = audioResources->Header.AssetCount;
        var hashes = (uint*)((byte*)audioResources + sizeof(AudioResourcesHeader));
        var resources = (AudioResource*)((byte*)hashes + sizeof(uint) * resCount);

        for (int i = 0; i < resCount; i++) {
            var hash = hashes[i];
            var resource = resources[i];
            var metadata = resource.AmtaOffset.GetPointer(resAudioResources);
            var asset = resource.AssetOffset.GetPointer(resAudioResources);

            bars[hash] = new BarsEntry {
                Metadata = AmtaSerializer.Deserialize(metadata, out _),
                Asset = new ReadOnlySpan<byte>(asset, ResourceHelper.GetAssetSize(asset)).ToArray(),
            };
        }

        return bars;
    }

    private static unsafe void SwapEndianness(AudioResources* resAudioResources)
    {
        if (!EndianUtils.ShouldSwap(resAudioResources->Header.Endianness)) {
            return;
        }

        AudioResourcesHeader.Swap(&resAudioResources->Header);

        var resCount = resAudioResources->Header.AssetCount;
        byte* pos = (byte*)resAudioResources + sizeof(AudioResourcesHeader);

        for (int i = 0; i < resCount; i++) {
            EndianUtils.Swap((uint*)pos);
            pos += sizeof(uint);
        }

        for (int i = 0; i < resCount; i++) {
            AudioResource.Swap((AudioResource*)pos);
            pos += sizeof(AudioResource);
        }
    }

    /// <summary>
    /// Swap the endianness from a system-matching endianness (backwards for serialization)
    /// </summary>
    private static unsafe void SwapEndiannessFromSystem(AudioResources* resAudioResources)
    {
        if (EndianUtils.ShouldSwap(resAudioResources->Header.Endianness)) {
            EndianUtils.Swap((ushort*)resAudioResources + 4);
            return;
        }

        var resCount = resAudioResources->Header.AssetCount;
        byte* pos = (byte*)resAudioResources + sizeof(AudioResourcesHeader);

        for (int i = 0; i < resCount; i++) {
            EndianUtils.Swap((uint*)pos);
            pos += sizeof(uint);
        }

        for (int i = 0; i < resCount; i++) {
            AudioResource.Swap((AudioResource*)pos);
            pos += sizeof(AudioResource);
        }

        AudioResourcesHeader.Swap(&resAudioResources->Header);
    }
}