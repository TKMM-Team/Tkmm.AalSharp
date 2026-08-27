using System.Runtime.CompilerServices;
using Entish;
using Tkmm.AalSharp.Helpers;

namespace Tkmm.AalSharp.Bars;

public sealed class AudioResourceAsset
{
    public string? Hint
        => Asset is [_, _, _, _, ..] ? PrimitivesHelper.ToAscii(Unsafe.As<byte, uint>(ref Asset[0])) : null;

    public required byte[] Metadata { get; set; }

    public required byte[]? Asset { get; set; }
    
    public int GetAlignment()
    {
        if (Asset is null || Asset.Length <= 8) {
            return 0x1;
        }

        var alignment = Unsafe.As<byte, short>(ref Asset[0x6]);
        if (EndianUtils.ShouldSwap(Unsafe.As<byte, Endianness>(ref Asset[0x4]))) {
            return EndianUtils.Swap(alignment);
        }
        
        return alignment;
    }
}