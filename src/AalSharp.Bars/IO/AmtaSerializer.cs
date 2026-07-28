using System.Collections.Frozen;
using System.Text;
using AalSharp.Bars.Data;
using AalSharp.Bars.IO.Data;
using Entish;

namespace AalSharp.Bars.IO;

public static class AmtaSerializer
{
    public static byte[] Serialize(BarsMetadata metadata, Endianness endianness = Endianness.Little)
    {
        var size = AudioMetadataParts.GetResSize(metadata);
        var buffer = new byte[size.Total];
        Serialize(metadata, buffer, size, endianness);
        return buffer;
    }

    public static unsafe void Serialize(BarsMetadata metadata, Span<byte> span, AudioMetadataParts size, Endianness endianness = Endianness.Little)
    {
        fixed (byte* ptr = span) {
            Serialize(metadata, (AudioMetadata*)ptr, size, endianness);
        }
    }

    public static unsafe void Serialize(BarsMetadata metadata, AudioMetadata* resAudioMetadata, AudioMetadataParts size, Endianness endianness = Endianness.Little)
    {
        var strings = BuildStringTable(metadata, out var includeNullString);
        
        *resAudioMetadata = new AudioMetadata {
            Header = new AudioMetadataHeader {
                Endianness = endianness,
                FileSize = size.Total
            },
            DataOffset = new Offset<AudioMetadataData>((uint)size.DataOffset),
            MarkerOffset = new Offset<AudioMetadataMarker>((uint)size.MarkerOffset),
            ExtOffset = new Offset<AudioMetadataExt>((uint)size.ExtOffset),
            StringTableOffset = new Offset<AudioMetadataStringTable>((uint)size.StringTableOffset),
        };

        var data = resAudioMetadata->DataOffset.GetPointer(resAudioMetadata);
        *data = new AudioMetadataData {
            SectionSize = size.DataSize,
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
        *marker = new AudioMetadataMarker {
            SectionSize = size.MarkerSize,
            NumEntries = metadata.Marker.Count
        };

        var markerEntries = (AudioMetadataMarkerEntry*)++marker;
        foreach (var entry in metadata.Marker) {
            *markerEntries = new AudioMetadataMarkerEntry {
                Id = entry.Id,
                NameOffset = entry.Name is null ? 0 : strings[entry.Name],
                StartPos = entry.StartPos,
                Length = entry.Length
            };

            markerEntries++;
        }

        var ext = resAudioMetadata->ExtOffset.GetPointer(resAudioMetadata);
        *ext = new AudioMetadataExt {
            SectionSize = size.ExtSize,
            NumEntries = metadata.Ext.Count
        };
        
        var extEntries = (AudioMetadataExtEntry*)++ext;
        foreach (var entry in metadata.Ext) {
            extEntries->Unknown[0] = entry.Unknown1;
            extEntries->Unknown[1] = entry.Unknown2;
            extEntries++;
        }
        
        var stringTable = resAudioMetadata->StringTableOffset.GetPointer(resAudioMetadata);
        *stringTable = new AudioMetadataStringTable();

        var stringTablePtr = (byte*)++stringTable;

        if (includeNullString) {
            WriteString(null, ref stringTablePtr);
        }

        foreach (var (str, _) in strings) {
            WriteString(str, ref stringTablePtr);
        }

        SwapEndiannessFromSystem(resAudioMetadata);
    }

    public static unsafe BarsMetadata Deserialize(void* resAudioMetadata, out Endianness endianness)
    {
        var audioMetadata = (AudioMetadata*)resAudioMetadata;
        endianness = EndianUtils.GetTrueEndianness(audioMetadata->Header.Endianness);

        SwapEndianness(audioMetadata);

        return new BarsMetadata {
            Data = Deserialize(audioMetadata->DataOffset.GetPointer(resAudioMetadata), audioMetadata),
            Marker = Deserialize(audioMetadata->MarkerOffset.GetPointer(resAudioMetadata), audioMetadata),
            Ext = Deserialize(audioMetadata->ExtOffset.GetPointer(resAudioMetadata)),
        };
    }

    private static unsafe BarsMetadataData Deserialize(AudioMetadataData* resData, AudioMetadata* metadata)
    {
        return new BarsMetadataData {
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
            StreamTracks = resData->GetStreamTracks().ToArray(),
            AmplitudePeak = resData->AmplitudePeak
        };
    }

    private static unsafe BarsMetadataMarker Deserialize(AudioMetadataMarker* resMarker, AudioMetadata* metadata)
    {
        var marker = new BarsMetadataMarker(resMarker->NumEntries);
        var entries = (AudioMetadataMarkerEntry*)(resMarker + 1);

        for (int i = 0; i < resMarker->NumEntries; i++) {
            var entry = entries[i];
            marker.Add(new BarsMetadataMarkerEntry {
                Id = entry.Id,
                Name = metadata->GetString(entry.NameOffset).ToString()
            });
        }

        return marker;
    }

    private static unsafe BarsMetadataExt Deserialize(AudioMetadataExt* resExt)
    {
        var ext = new BarsMetadataExt(resExt->NumEntries);
        var entries = (AudioMetadataExtEntry*)(resExt + 1);

        for (int i = 0; i < resExt->NumEntries; i++) {
            var entry = entries[i];
            ext.Add(new BarsMetadataExtEntry {
                Unknown1 = entry.Unknown[0],
                Unknown2 = entry.Unknown[1]
            });
        }

        return ext;
    }

    private static unsafe void SwapEndianness(AudioMetadata* resAudioMetadata)
    {
        if (!EndianUtils.ShouldSwap(resAudioMetadata->Header.Endianness)) {
            return;
        }

        AudioMetadata.Swap(resAudioMetadata);
        AudioMetadataData.Swap(resAudioMetadata->DataOffset.GetPointer(resAudioMetadata));

        var marker = resAudioMetadata->MarkerOffset.GetPointer(resAudioMetadata);
        var markerEntries = (AudioMetadataMarkerEntry*)++marker;
        AudioMetadataMarker.Swap(marker);

        for (int i = 0; i < marker->NumEntries; i++) {
            AudioMetadataMarkerEntry.Swap(++markerEntries);
        }

        var ext = resAudioMetadata->ExtOffset.GetPointer(resAudioMetadata);
        var extEntries = (AudioMetadataExtEntry*)++ext;
        AudioMetadataExt.Swap(ext);

        for (int i = 0; i < marker->NumEntries; i++) {
            AudioMetadataExtEntry.Swap(++extEntries);
        }

        var stringTable = resAudioMetadata->StringTableOffset.GetPointer(resAudioMetadata);
        AudioMetadataStringTable.Swap(stringTable, resAudioMetadata->Header.FileSize - (int)((byte*)stringTable - (byte*)resAudioMetadata));
    }

    private static unsafe void SwapEndiannessFromSystem(AudioMetadata* resAudioMetadata)
    {
        if (EndianUtils.ShouldSwap(resAudioMetadata->Header.Endianness)) {
            return;
        }

        AudioMetadataData.Swap(resAudioMetadata->DataOffset.GetPointer(resAudioMetadata));
        
        var marker = resAudioMetadata->MarkerOffset.GetPointer(resAudioMetadata);
        var markerEntries = (AudioMetadataMarkerEntry*)++marker;

        for (int i = 0; i < marker->NumEntries; i++) {
            AudioMetadataMarkerEntry.Swap(++markerEntries);
        }
        
        AudioMetadataMarker.Swap(marker);

        var ext = resAudioMetadata->ExtOffset.GetPointer(resAudioMetadata);
        var extEntries = (AudioMetadataExtEntry*)++ext;

        for (int i = 0; i < marker->NumEntries; i++) {
            AudioMetadataExtEntry.Swap(++extEntries);
        }
        
        AudioMetadataExt.Swap(ext);

        var stringTable = resAudioMetadata->StringTableOffset.GetPointer(resAudioMetadata);
        AudioMetadataStringTable.Swap(stringTable, resAudioMetadata->Header.FileSize - (int)((byte*)resAudioMetadata - (byte*)stringTable));
        
        AudioMetadata.Swap(resAudioMetadata);
    }

    public static IEnumerable<string?> GetStrings(BarsMetadata metadata)
    {
        yield return metadata.Data.Name;

        foreach (var marker in metadata.Marker) {
            yield return marker.Name;
        }
    }

    private static FrozenDictionary<string, int> BuildStringTable(BarsMetadata metadata, out bool includeNullString)
    {
        int rollingOffset = 0;
        Dictionary<string, int> stringTable = new();

        includeNullString = false;
        
        foreach (var str in GetStrings(metadata).Distinct().Order()) {
            if (str is null) {
                includeNullString = true;
                continue;
            }
            
            stringTable.Add(str, rollingOffset);
            rollingOffset += Encoding.UTF8.GetByteCount(str) + 1;
        }

        return stringTable.ToFrozenDictionary();
    }
    
    private static unsafe void WriteString(string? str, ref byte* ptr)
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