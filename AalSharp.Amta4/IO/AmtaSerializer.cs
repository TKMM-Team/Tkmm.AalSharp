using System.Collections.Frozen;
using System.Text;
using AalSharp.Amta.Data;
using Entish;

namespace AalSharp.Amta.IO;

public unsafe class AmtaSerializer : IAmtaSerializer
{
    public void Serialize(IAudioMetadata metadata, Span<byte> span, IResourceSize size, Endianness endianness = Endianness.Little)
        => Serialize((AalAudioMetadata)metadata, span, (ResAudioMetadataSize)size, endianness);
    
    public void Serialize(IAudioMetadata metadata, void* resAudioMetadata, IResourceSize size, Endianness endianness = Endianness.Little)
        => Serialize((AalAudioMetadata)metadata, (ResAudioMetadata*)resAudioMetadata, (ResAudioMetadataSize)size, endianness);

    public static byte[] Serialize(AalAudioMetadata metadata, Endianness endianness = Endianness.Little)
    {
        var size = new ResAudioMetadataSize(metadata);
        var buffer = new byte[size.Total];
        Serialize(metadata, buffer, size, endianness);
        return buffer;
    }

    public static unsafe void Serialize(AalAudioMetadata metadata, Span<byte> span, ResAudioMetadataSize size, Endianness endianness = Endianness.Little)
    {
        fixed (byte* ptr = span) {
            Serialize(metadata, (ResAudioMetadata*)ptr, size, endianness);
        }
    }

    public static unsafe void Serialize(AalAudioMetadata metadata, ResAudioMetadata* resAudioMetadata, ResAudioMetadataSize size, Endianness endianness = Endianness.Little)
    {
        var strings = BuildStringTable(metadata, out var includeNullString);

        *resAudioMetadata = new ResAudioMetadata {
            Endianness = endianness,
            FileSize = size.Total,
            DataOffset = new Offset<ResData>(size.DataOffset),
            MarkerOffset = new Offset<ResContainer>(size.MarkerOffset),
            ExtOffset = new Offset<ResContainer>(size.ExtOffset),
            StringTableOffset = new Offset<ResStringTable>(size.StringTableOffset),
        };

        var data = resAudioMetadata->DataOffset.GetPointer(resAudioMetadata);
        *data = new ResData {
            SectionSize = size.DataSize - 0x8,
            NameOffset = metadata.Data.Name is null ? 0 : strings[metadata.Data.Name],
            SampleCount = metadata.Data.SampleCount,
            Type = metadata.Data.Type,
            WaveChannels = metadata.Data.WaveChannels,
            UsedStreamTracks = metadata.Data.UsedStreamTracks,
            Flags = metadata.Data.Flags,
            Duration = metadata.Data.Duration,
            SampleRate = metadata.Data.SampleRate,
            LoopStartSample = metadata.Data.LoopStartSample,
            LoopEndSample = metadata.Data.LoopEndSample,
            Loudness = metadata.Data.Loudness,
            AmplitudePeak = metadata.Data.AmplitudePeak
        };

        var streamTracks = data->GetStreamTracks();
        for (int i = 0; i < streamTracks.Length; i++) {
            streamTracks[i] = metadata.Data.StreamTracks[i];
        }

        var marker = resAudioMetadata->MarkerOffset.GetPointer(resAudioMetadata);
        *marker = new ResContainer {
            Magic = ResAudioMetadata.AmtaAttributesMagic,
            SectionSize = size.MarkerSize - 0x8,
            NumEntries = metadata.Markers.Count
        };

        var markerEntries = (ResMarker*)++marker;
        foreach (var entry in metadata.Markers) {
            *markerEntries = new ResMarker {
                Id = entry.Id,
                NameOffset = entry.Name is null ? 0 : strings[entry.Name],
                StartPos = entry.StartPos,
                Length = entry.Length
            };

            markerEntries++;
        }

        var ext = resAudioMetadata->ExtOffset.GetPointer(resAudioMetadata);
        *ext = new ResContainer {
            SectionSize = size.ExtSize - 0x8,
            NumEntries = metadata.Attributes.Count
        };

        var extEntries = (ResAttribute*)++ext;
        foreach (var entry in metadata.Attributes) {
            extEntries->KeyOffset = entry.Key is null ? 0 : strings[entry.Key];
            extEntries->Value = entry.Value;
            extEntries++;
        }

        var stringTable = resAudioMetadata->StringTableOffset.GetPointer(resAudioMetadata);
        *stringTable = new ResStringTable();

        var stringTablePtr = (byte*)++stringTable;

        if (includeNullString) {
            WriteString(null, ref stringTablePtr);
        }

        foreach (var (str, _) in strings) {
            WriteString(str, ref stringTablePtr);
        }

        SwapEndiannessFromSystem(resAudioMetadata);
    }

    public static IAudioMetadata Deserialize(void* resAudioMetadata, out Endianness endianness)
    {
        var audioMetadata = (ResAudioMetadata*)resAudioMetadata;
        endianness = EndianUtils.GetTrueEndianness(audioMetadata->Endianness);

        SwapEndianness(audioMetadata);

        return new AalAudioMetadata {
            Data = Deserialize(audioMetadata->DataOffset.GetPointer(resAudioMetadata), audioMetadata),
            Markers = DeserializeMarkers(audioMetadata->MarkerOffset.GetPointer(resAudioMetadata), audioMetadata),
            Attributes = DeserializeAttributes(audioMetadata->ExtOffset.GetPointer(resAudioMetadata), audioMetadata),
        };
    }

    public static unsafe AalOptionalMetadata Deserialize(ResData* resData, ResAudioMetadata* metadata)
    {
        return new AalOptionalMetadata {
            Name = metadata->Name.ToString(),
            SampleCount = resData->SampleCount,
            Type = resData->Type,
            WaveChannels = resData->WaveChannels,
            UsedStreamTracks = resData->UsedStreamTracks,
            Flags = resData->Flags,
            Duration = resData->Duration,
            SampleRate = resData->SampleRate,
            LoopStartSample = resData->LoopStartSample,
            LoopEndSample = resData->LoopEndSample,
            Loudness = resData->Loudness,
            StreamTracks = [.. resData->GetStreamTracks()],
            AmplitudePeak = resData->AmplitudePeak
        };
    }

    public static unsafe List<AalMarker> DeserializeMarkers(ResContainer* resMarker, ResAudioMetadata* metadata)
    {
        var marker = new List<AalMarker>(resMarker->NumEntries);
        var entries = (ResMarker*)(resMarker + 1);

        for (int i = 0; i < resMarker->NumEntries; i++) {
            var entry = entries[i];
            marker.Add(new AalMarker {
                Id = entry.Id,
                Name = metadata->GetString(entry.NameOffset).ToString(),
                StartPos = entry.StartPos,
                Length = entry.Length
            });
        }

        return marker;
    }

    public static unsafe List<AalAttribute> DeserializeAttributes(ResContainer* resExt, ResAudioMetadata* metadata)
    {
        var ext = new List<AalAttribute>(resExt->NumEntries);
        var entries = (ResAttribute*)(resExt + 1);

        for (int i = 0; i < resExt->NumEntries; i++) {
            var entry = entries[i];
            ext.Add(new AalAttribute {
                Key = metadata->GetString(entry.KeyOffset).ToString(),
                Value = entry.Value
            });
        }

        return ext;
    }

    public static unsafe void SwapEndianness(ResAudioMetadata* resAudioMetadata)
    {
        if (!EndianUtils.ShouldSwap(resAudioMetadata->Endianness)) {
            return;
        }

        ResAudioMetadata.Swap(resAudioMetadata);
        ResData.Swap(resAudioMetadata->DataOffset.GetPointer(resAudioMetadata));

        var marker = resAudioMetadata->MarkerOffset.GetPointer(resAudioMetadata);
        var markerEntries = (ResMarker*)(marker + 1);
        ResContainer.Swap(marker);

        for (int i = 0; i < marker->NumEntries; i++) {
            ResMarker.Swap(markerEntries++);
        }

        var ext = resAudioMetadata->ExtOffset.GetPointer(resAudioMetadata);
        var extEntries = (ResAttribute*)(ext + 1);
        ResContainer.Swap(ext);

        for (int i = 0; i < ext->NumEntries; i++) {
            ResAttribute.Swap(extEntries++);
        }

        var stringTable = resAudioMetadata->StringTableOffset.GetPointer(resAudioMetadata);
        ResStringTable.Swap(stringTable, resAudioMetadata->FileSize - (int)((byte*)stringTable - (byte*)resAudioMetadata));
    }

    public static unsafe void SwapEndiannessFromSystem(ResAudioMetadata* resAudioMetadata)
    {
        if (EndianUtils.ShouldSwap(resAudioMetadata->Endianness)) {
            EndianUtils.Swap((ushort*)resAudioMetadata + 2);
            return;
        }

        ResData.Swap(resAudioMetadata->DataOffset.GetPointer(resAudioMetadata));

        var marker = resAudioMetadata->MarkerOffset.GetPointer(resAudioMetadata);
        var markerEntries = (ResMarker*)(marker + 1);

        for (int i = 0; i < marker->NumEntries; i++) {
            ResMarker.Swap(markerEntries++);
        }

        ResContainer.Swap(marker);

        var ext = resAudioMetadata->ExtOffset.GetPointer(resAudioMetadata);
        var extEntries = (ResAttribute*)(ext + 1);

        for (int i = 0; i < ext->NumEntries; i++) {
            ResAttribute.Swap(extEntries++);
        }

        ResContainer.Swap(ext);

        var stringTable = resAudioMetadata->StringTableOffset.GetPointer(resAudioMetadata);
        ResStringTable.SwapFromSystem(stringTable, resAudioMetadata->FileSize - (int)((byte*)stringTable - (byte*)resAudioMetadata));

        ResAudioMetadata.Swap(resAudioMetadata);
    }

    public static IEnumerable<string?> GetStrings(AalAudioMetadata metadata)
    {
        yield return metadata.Data.Name;

        foreach (var marker in metadata.Markers) {
            yield return marker.Name;
        }
    }

    public static FrozenDictionary<string, int> BuildStringTable(AalAudioMetadata metadata, out bool includeNullString)
    {
        int rollingOffset = 0;
        Dictionary<string, int> stringTable = new();

        includeNullString = false;

        foreach (var str in GetStrings(metadata).Distinct().Order(StringComparer.Ordinal)) {
            if (str is null) {
                includeNullString = true;
                continue;
            }

            stringTable.Add(str, rollingOffset);
            rollingOffset += Encoding.UTF8.GetByteCount(str) + 5;
        }

        return stringTable.ToFrozenDictionary();
    }

    public static unsafe void WriteString(string? str, ref byte* ptr)
    {
        if (str is null) {
            // u32(size), byte(null), byte(padding)
            *(int*)ptr = 0x1;
            ptr += sizeof(uint) + 2;
            return;
        }

        var len = Encoding.UTF8.GetByteCount(str);
        *(int*)ptr = len + 1;

        ptr += sizeof(int);

        var span = new Span<byte>(ptr, len);
        Encoding.UTF8.GetBytes(str, span);
        ptr += len + 1;
    }
}