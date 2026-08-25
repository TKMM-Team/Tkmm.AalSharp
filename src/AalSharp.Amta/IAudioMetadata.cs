using AalSharp.Amta.IO;

namespace AalSharp.Amta;

/// <summary>
/// Represents audio metadata (AMTA) 
/// </summary>
public interface IAudioMetadata
{
    IResourceSize GetResSize();

    IAmtaSerializer GetSerializer();
}