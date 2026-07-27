using AalSharp.Bars.Data;
using Entish;

namespace AalSharp.Bars.IO.Data;

public unsafe struct AudioMetadataParts
{
    public int HeaderSize;
    public int DataOffset;
    public int DataSize;
    public int MarkerOffset;
    public int MarkerSize;
    public int ExtOffset;
    public int ExtSize;
    public int StringTableOffset;
    public int StringTableSize;
    public int Total;

    public static AudioMetadataParts GetResSize(BarsMetadata metadata)
    {
        var headerSize = sizeof(AudioMetadata);
        var dataSize = sizeof(AudioMetadataData);
        var markerOffset = headerSize + dataSize;
        var markerSize = sizeof(AudioMetadataMarker) + metadata.Marker.Count * sizeof(AudioMetadataMarker);
        var extOffset = markerOffset + markerSize;
        var extSize = sizeof(AudioMetadataExt) + metadata.Marker.Count * sizeof(AudioMetadataExtEntry);
        var stringTableOffset = extOffset + extSize;
        var stringTableSize = sizeof(AudioMetadataStringTable) + AmtaSerializer
            .GetStrings(metadata)
            .Distinct()
            .Sum(str => (str?.Length ?? 0) + 5);

        return new AudioMetadataParts {
            HeaderSize = headerSize,
            DataOffset = headerSize,
            DataSize = dataSize,
            MarkerOffset = markerOffset,
            MarkerSize = markerSize,
            ExtOffset = extOffset,
            ExtSize = extSize,
            StringTableOffset = stringTableOffset,
            StringTableSize = stringTableOffset,
            Total = (stringTableOffset + stringTableSize).AlignUp(0x4)
        };
    }
}