using System.Runtime.CompilerServices;
using AalSharp.Helpers;

namespace AalSharp.Bars;

public sealed class BarsEntry
{
    public string? Hint
        => Asset is [_, _, _, _, ..] ? PrimitivesHelper.ToAscii(Unsafe.As<byte, uint>(ref Asset[0])) : null;

    public required BarsMetadata Metadata { get; set; }

    public required byte[] Asset { get; set; }
}