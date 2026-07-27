using AalSharp.Bars.Data;

namespace AalSharp.Bars;

public sealed class BarsMetadataData
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

    public AudioMetadataStreamTrack[] StreamTracks { get; set; } = new AudioMetadataStreamTrack[8];

    public float AmplitudePeak { get; set; }
}