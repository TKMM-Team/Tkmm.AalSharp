using System.Runtime.InteropServices;
using AalSharp.Bars.Data;
using AalSharp.Bars.IO.Data;
using AalSharp.Helpers;
using Entish;

namespace AalSharp.Bars.IO;

public sealed class BarsSerializer
{
    public const int AssetAlignment = 0x40;
    
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
        // TODO: Remove duplicate files
        
        *resAudioResources = new AudioResources {
            Header = new AudioResourcesHeader {
                FileSize = (uint)size.Total,
                AssetCount = bars.Count,
                Endianness = endianness
            }
        };

        // Swap expecting LE write
        if (BitConverter.IsLittleEndian) {
            EndianUtils.Swap((ushort*)resAudioResources + 4);
        }

        var hashes = (uint*)(resAudioResources + 1);
        foreach (uint hash in bars.Keys.Order()) {
            *hashes = hash;
            hashes++;
        }

        var resources = (AudioResource*)hashes;
        var metadata = (AudioMetadata*)((byte*)resAudioResources + size.MetadataOffset);
        var assetData = (byte*)resAudioResources + size.AssetOffset;

        foreach (var (_, entry) in bars.OrderBy(static entry => entry.Key)) {
            *resources = new AudioResource {
                AmtaOffset = new Offset<byte>((uint)size.MetadataOffset),
                AssetOffset = new Offset<byte>((uint)size.AssetOffset),
            };

            var metadataSize = AudioMetadataParts.GetResSize(entry.Metadata);
            AmtaSerializer.Serialize(entry.Metadata, metadata, metadataSize, endianness);
            
            Marshal.Copy(entry.Asset, 0, (IntPtr)assetData, entry.Asset.Length);
            
            resources++;
            metadata = (AudioMetadata*)((byte*)metadata + metadataSize.Total);
            size.MetadataOffset += metadataSize.Total;

            int assetBlockSize = entry.Asset.Length.AlignUp(AssetAlignment); 
            assetData += assetBlockSize;
            size.AssetOffset += assetBlockSize;
        }

        SwapEndiannessFromSystem(resAudioResources);
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

    /// <summary>
    /// Swap the endianness from a system-matching endianness (backwards for serialization)
    /// </summary>
    private static unsafe void SwapEndiannessFromSystem(AudioResources* resAudioResources)
    {
        if (!EndianUtils.ShouldSwap(resAudioResources->Header.Endianness)) {
            return;
        }
        
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
        
        AudioResourcesHeader.Swap(&resAudioResources->Header);
    }
}