using Entish;

namespace Tkmm.AalSharp.Bars.Data;

public readonly struct ResAudioMetadataHeader
{
    public const uint AmtaMagic = 0x41544D41;

    public readonly uint Magic;
    public readonly Endianness Endianness;
    public readonly ushort Version;
    public readonly int FileSize;
}