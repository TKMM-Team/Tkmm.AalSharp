using Entish.Attributes;

namespace AalSharp.Bars.Data;

public enum AudioMetadataDataType : byte
{
    Wave,
    Stream
};

[Swappable]
public unsafe partial struct AudioMetadataData()
{
    public const uint AmtaDataMagic = 0x41544144;

    [NeverSwap]
    public readonly uint Magic = AmtaDataMagic;
    public int SectionSize;
    public int NameOffset;
    public uint SampleCount;
    public AudioMetadataDataType Type;
    public byte WaveChannels;
    public byte UsedStreamTracks;
    public byte Flags;
    public uint Duration;
    public uint SampleRate;
    public uint LoopStartSample;
    public uint LoopEndSample;
    public float Loudness;
    private fixed uint _streamTracks[16]; // (uint, float) x8 
    public float AmplitudePeak;

    public Span<AudioMetadataStreamTrack> GetStreamTracks()
    {
        fixed (void* ptr = _streamTracks) {
            return new Span<AudioMetadataStreamTrack>(ptr, 8);
        }
    }
}