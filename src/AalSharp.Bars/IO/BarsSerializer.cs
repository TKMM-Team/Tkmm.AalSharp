using System.Runtime.InteropServices;
using AalSharp.Bars.Data;
using AalSharp.Helpers;
using Entish;

namespace AalSharp.Bars.IO;

public sealed class BarsSerializer
{
    public static unsafe byte[] Serialize(BarsFile bars, Endianness endianness = Endianness.Little, ushort version = 0x101, Func<int, byte[]>? alloc = null)
    {
        var headerSize = sizeof(AudioResourcesHeader);
        var hashesOffset = headerSize;
        var hashesSize = sizeof(uint) * bars.Count;
        var resourcesOffset = hashesOffset + hashesSize;
        var resourcesSize = sizeof(AudioResource) * bars.Count;
        var metadataOffset = resourcesOffset + resourcesSize;
        var metadataSize = bars.Values.Sum(static entry => entry.Asset.Length);
        var assetOffset = metadataOffset + metadataSize;
        var assetSize = bars.Values.Sum(static entry => entry.Asset.Length);

        var size = assetOffset + assetSize;
        var buffer = alloc?.Invoke(size) ?? new byte[size];
        var span = buffer.AsSpan();

        var header = new AudioResourcesHeader {
            FileSize = (uint)size,
            AssetCount = bars.Count,
            Endianness = endianness,
            Version = version
        };
        
        MemoryMarshal.Write(span[..headerSize], header);

        foreach (uint hash in bars.Keys.Order()) {
            MemoryMarshal.Write(span[hashesOffset..(hashesOffset += sizeof(uint))], hash);
        }

        foreach (var (_, entry) in bars.OrderBy(static entry => entry.Key)) {
            MemoryMarshal.Write(span[resourcesOffset..(resourcesOffset += sizeof(AudioResource))], new AudioResource {
                AmtaOffset = new Offset<byte>((uint)metadataOffset),
                AssetOffset = new Offset<byte>((uint)assetOffset),
            });

            entry.Metadata.CopyTo(span[metadataOffset..(metadataOffset += entry.Metadata.Length)]);
            entry.Asset.CopyTo(span[assetOffset..(assetOffset += entry.Asset.Length)]);
        }

        return buffer;
    }

    public static unsafe BarsFile Deserialize(void* resAudioResources)
    {
        BarsFile bars = [];
        
        var audioResources = (AudioResources*)resAudioResources;
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
                Hint = PrimitivesHelper.ToAscii(*(uint*)asset),
                Metadata = AmtaSerializer.Deserialize(metadata),
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
            pos += sizeof(uint*);
        }

        for (int i = 0; i < resCount; i++) {
            AudioResource.Swap((AudioResource*)pos);
            pos += sizeof(AudioResource*);
        }
    }
}