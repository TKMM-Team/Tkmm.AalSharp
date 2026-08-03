using AalSharp.Amta.IO;

namespace AalSharp.Amta;

public sealed class AalAudioMetadata : IAudioMetadata
{
    public required AalOptionalMetadata Data { get; set; }

    public List<AalMarker> Markers { get; set; } = [];

    public List<AalAttribute> Attributes { get; set; } = [];

    public IResourceSize GetResSize() => new ResAudioMetadataSize(this);

    public IAmtaSerializer GetSerializer() => new AmtaSerializer();
}