using System.Collections.Frozen;
using System.Text;
using AalSharp.Bars.Data;
using AalSharp.Bars.IO.Data;
using Entish;

namespace AalSharp.Bars.IO;

public sealed class AmtaSerializer
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
        throw new NotImplementedException();
    }

    public static unsafe BarsMetadata Deserialize(void* resAudioMetadata)
    {
        var audioMetadata = (AudioMetadata*)resAudioMetadata;
        SwapEndianness(audioMetadata);

        return new BarsMetadata {
            Version = audioMetadata->Header.Version,
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
        AudioMetadataStringTable.Swap(stringTable, resAudioMetadata->Header.FileSize - (int)((byte*)resAudioMetadata - (byte*)stringTable));
    }
}