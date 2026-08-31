using System.Runtime.InteropServices;
using Entish;

namespace Tkmm.AalSharp.Bars.Data;

[StructLayout(LayoutKind.Sequential, Pack = 2)]
public readonly struct ResAudioMetadataHeader
{
    public readonly uint Magic;
    public readonly Endianness Endianness;
    public readonly ushort Version;
    public readonly int FileSize;
}