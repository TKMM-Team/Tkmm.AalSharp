using System.Runtime.CompilerServices;
using AalSharp.Helpers;
using Entish;

namespace AalSharp.Bars;

public sealed class BarsEntry
{
    public string? Hint
        => Asset is [_, _, _, _, ..] ? PrimitivesHelper.ToAscii(Unsafe.As<byte, uint>(ref Asset[0])) : null;

    public required BarsMetadata Metadata { get; set; }

    public required byte[] Asset { get; set; }
    
    public int GetAlignment()
    {
        if (Asset.Length <= 8) {
            throw new InvalidDataException("Invalid sound asset file.");
        }

        var alignment = Unsafe.As<byte, short>(ref Asset[0x6]);
        if (EndianUtils.ShouldSwap(Unsafe.As<byte, Endianness>(ref Asset[0x4]))) {
            return EndianUtils.Swap(alignment);
        }
        
        return alignment;
    }
}