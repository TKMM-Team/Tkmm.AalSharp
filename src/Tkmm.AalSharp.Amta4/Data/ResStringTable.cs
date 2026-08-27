using Entish;
using Entish.Attributes;

namespace Tkmm.AalSharp.Amta.Data;

public struct ResStringTable()
{
    public const uint AmtaStringTableMagic = 1196577875;

    [NeverSwap]
    public readonly uint Magic = AmtaStringTableMagic;
    
    public static unsafe void Swap(ResStringTable* value, int size)
    {
        var ptr = (byte*)value + sizeof(ResStringTable);

        for (int i = sizeof(ResStringTable); i + 0x4 < size;) {
            var len = (int*)ptr;
            EndianUtils.Swap(len);

            var stringSize = 0x4 + *len;
            ptr += stringSize;
            i += stringSize;
        }
    }
    
    public static unsafe void SwapFromSystem(ResStringTable* value, int size)
    {
        var ptr = (byte*)value + sizeof(ResStringTable);

        for (int i = sizeof(ResStringTable); i + 0x4 < size;) {
            var len = (int*)ptr;
            var stringSize = 0x4 + *len;
            
            EndianUtils.Swap(len);

            ptr += stringSize;
            i += stringSize;
        }
    }
}