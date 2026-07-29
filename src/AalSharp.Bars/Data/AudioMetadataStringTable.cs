using Entish;
using Entish.Attributes;

namespace AalSharp.Bars.Data;

public struct AudioMetadataStringTable()
{
    public const uint AmtaStringTableMagic = 1196577875;

    [NeverSwap]
    public readonly uint Magic = AmtaStringTableMagic;
    
    public static unsafe void Swap(AudioMetadataStringTable* value, int size)
    {
        var ptr = (byte*)value + sizeof(AudioMetadataStringTable);

        for (int i = sizeof(AudioMetadataStringTable); i + 0x4 < size;) {
            var len = (int*)ptr;
            EndianUtils.Swap(len);

            var stringSize = 0x4 + *len;
            ptr += stringSize;
            i += stringSize;
        }
    }
}