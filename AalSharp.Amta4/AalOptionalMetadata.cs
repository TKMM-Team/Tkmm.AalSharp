using AalSharp.Amta.Data;

namespace AalSharp.Amta;

public sealed class AalOptionalMetadata
{
    public string? Name { get; set; }

    public uint SampleCount { get; set; }

    public AudioMetadataDataType Type { get; set; }

    public byte WaveChannels { get; set; }

    public byte UsedStreamTracks { get; set; }

    public byte Flags { get; set; }

    public uint Duration { get; set; }

    public uint SampleRate { get; set; }

    public uint LoopStartSample { get; set; }

    public uint LoopEndSample { get; set; }

    public float Loudness { get; set; }

    public ResStreamTrack[] StreamTracks { get; set; } = new ResStreamTrack[8];

    public float AmplitudePeak { get; set; }
}