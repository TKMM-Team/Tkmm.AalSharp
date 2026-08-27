using Entish;

namespace Tkmm.AalSharp.Amta.IO;

/// <summary>
/// Version agnostic serialization functions for audio metadata (AMTA)
/// </summary>
public unsafe interface IAmtaSerializer
{
    static abstract IAudioMetadata Deserialize(void* resAudioMetadata, out Endianness endianness);
    
    void Serialize(IAudioMetadata metadata, Span<byte> span, IResourceSize size, Endianness endianness = Endianness.Little);
    
    void Serialize(IAudioMetadata metadata, void* resAudioMetadata, IResourceSize size, Endianness endianness = Endianness.Little);
}