using Entish.Attributes;

namespace AalSharp.Bars.Data;

[Swappable]
public partial struct AudioMetadataStreamTrack
{
    public uint ChannelCount;
    public float Volume;
};