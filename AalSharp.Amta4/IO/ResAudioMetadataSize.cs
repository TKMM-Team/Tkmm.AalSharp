using AalSharp.Amta.Data;
using Entish;

namespace AalSharp.Amta.IO;

public unsafe struct ResAudioMetadataSize : IResourceSize
{
    public readonly int HeaderSize;
    public readonly int DataOffset;
    public readonly int DataSize;
    public readonly int MarkerOffset;
    public readonly int MarkerSize;
    public readonly int ExtOffset;
    public readonly int ExtSize;
    public readonly int StringTableOffset;
    public readonly int StringTableSize;
    
    public int Total { get; }

    public ResAudioMetadataSize(AalAudioMetadata metadata)
    {
        HeaderSize = sizeof(ResAudioMetadata);
        DataOffset = HeaderSize;
        DataSize = sizeof(ResData);
        MarkerOffset = HeaderSize + DataSize;
        MarkerSize = sizeof(ResMarker) + metadata.Markers.Count * sizeof(ResMarker);
        ExtOffset = MarkerOffset + MarkerSize;
        ExtSize = sizeof(ResContainer) + metadata.Attributes.Count * sizeof(ResAttribute);
        StringTableOffset = ExtOffset + ExtSize;
        StringTableSize = sizeof(ResStringTable) + AmtaSerializer
            .GetStrings(metadata)
            .Distinct()
            .Sum(str => (str?.Length ?? 0) + 5);

        Total = (StringTableOffset + StringTableSize).AlignUp(0x4);
    }
}