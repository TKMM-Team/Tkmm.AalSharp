using Entish;

namespace AalSharp.Bars.Data;

public struct AudioMetadataStringTable()
{
    public const uint AmtaStringTableMagic = 1196577875;

    public readonly uint Magic = AmtaStringTableMagic;
    
    public static unsafe void Swap(AudioMetadataStringTable* value, int size)
    {
        var ptr = (byte*)value;

        for (int i = sizeof(AudioMetadataStringTable); i < size; i++) {
            var len = (int*)ptr;
            EndianUtils.Swap(len);
            
            ptr += *len;
            i += *len;
        }
    }
}