using Tkmm.AalSharp.Amta.IO;

namespace Tkmm.AalSharp.Amta;

/// <summary>
/// Represents audio metadata (AMTA) 
/// </summary>
public interface IAudioMetadata
{
    IResourceSize GetResSize();

    IAmtaSerializer GetSerializer();
}